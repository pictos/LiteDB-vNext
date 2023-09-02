﻿namespace LiteDB.Engine;

[AutoInterface(typeof(IDisposable))]
unsafe internal class DiskStream : IDiskStream
{
    private readonly IEngineSettings _settings;
    private readonly IStreamFactory _streamFactory;

    private Stream? _stream;
    private Stream? _contentStream;

    public string Name => Path.GetFileName(_settings.Filename);

    public DiskStream(
        IEngineSettings settings, 
        IStreamFactory streamFactory)
    {
        _settings = settings;
        _streamFactory = streamFactory;
    }

    /// <summary>
    /// Initialize disk opening already exist datafile and return file header structure.
    /// Can open file as read or write
    /// </summary>
    public FileHeader OpenFile(bool canWrite)
    {
        // get a new FileStream connected to file
        _stream = _streamFactory.GetStream(canWrite,
            FileOptions.RandomAccess);

        // reading file header
        using var buffer = SharedBuffer.Rent(FILE_HEADER_SIZE);

        _stream.Position = 0;

        var read = _stream.Read(buffer.AsSpan());

        ENSURE(read != PAGE_HEADER_SIZE, new { read });

        var header = new FileHeader(buffer.AsSpan());

        // for content stream, use AesStream (for encrypted file) or same _stream
        _contentStream = header.Encrypted ?
            new AesStream(_stream, _settings.Password ?? "", header.EncryptionSalt) :
            _stream;

        return header;
    }

    /// <summary>
    /// Open stream with no FileHeader read (need FileHeader instance)
    /// </summary>
    public void OpenFile(FileHeader header)
    {
        // get a new FileStream connected to file
        _stream = _streamFactory.GetStream(false, FileOptions.RandomAccess);

        // for content stream, use AesStream (for encrypted file) or same _stream
        _contentStream = header.Encrypted ?
            new AesStream(_stream, _settings.Password ?? "", header.EncryptionSalt) :
            _stream;
    }

    /// <summary>
    /// Initialize disk creating a new datafile and writing file header
    /// </summary>
    public void CreateNewFile(FileHeader fileHeader)
    {
        // create new data file
        _stream = _streamFactory.GetStream(true, FileOptions.SequentialScan);

        // writing file header
        _stream.Position = 0;

        _stream.Write(fileHeader.ToArray());

        // for content stream, use AesStream (for encrypted file) or same _stream
        _contentStream = fileHeader.Encrypted ?
            new AesStream(_stream, _settings.Password ?? "", fileHeader.EncryptionSalt) :
            _stream;
    }

    public Task FlushAsync()
    {
        return _contentStream?.FlushAsync() ?? Task.CompletedTask;
    }

    /// <summary>
    /// Calculate, using disk file length, last PositionID. Should considering FILE_HEADER_SIZE and celling pages.
    /// </summary>
    public int GetLastFilePositionID()
    {
        var fileLength = _streamFactory.GetLength();

        // fileLength must be, at least, FILE_HEADER
        if (fileLength <= FILE_HEADER_SIZE) throw ERR($"Invalid datafile. Data file is too small (length = {fileLength}).");

        var content = fileLength - FILE_HEADER_SIZE;
        var celling = content % PAGE_SIZE > 0 ? 1 : 0;
        var result = content / PAGE_SIZE;

        // if last page was not completed written, add missing bytes to complete

        return (int)(result + celling - 1);
    }

    /// <summary>
    /// Read single page from disk using disk position. Load header instance too. This position has FILE_HEADER_SIZE offset
    /// </summary>
    public bool ReadPage(PageMemory* pagePtr, uint positionID)
    {
        using var _pc = PERF_COUNTER(2, nameof(ReadPage), nameof(DiskStream));

        ENSURE(positionID != uint.MaxValue, "PositionID should not be empty");

        // set real position on stream
        _contentStream!.Position = FILE_HEADER_SIZE + (positionID * PAGE_SIZE);

        var span = new Span<byte>(pagePtr, PAGE_SIZE);

        // read uniqueID to restore after read from disk
        var uniqueID = pagePtr->UniqueID;

        var read = _contentStream.Read(span);

        pagePtr->UniqueID = uniqueID;

        ENSURE(pagePtr->PositionID == positionID, "is temp page?");

        return read == PAGE_SIZE;
    }

    public void WritePage(PageMemory* pagePtr)
    {
        using var _pc = PERF_COUNTER(3, nameof(WritePage), nameof(DiskStream));

        ENSURE(pagePtr->IsDirty);
        ENSURE(pagePtr->ShareCounter == NO_CACHE);
        ENSURE(pagePtr->PositionID != int.MaxValue);

        // update crc8 page
        pagePtr->Crc8 = 0; // pagePtr->ComputeCrc8();

        // set real position on stream
        _contentStream!.Position = FILE_HEADER_SIZE + (pagePtr->PositionID * PAGE_SIZE);

        var span = new Span<byte>(pagePtr, PAGE_SIZE);

        _contentStream.Write(span);

        // clear isDirty flag
        pagePtr->IsDirty = false;
    }

    /// <summary>
    /// Write an empty (full \0) PAGE_SIZE using positionID
    /// </summary>
    public void WriteEmptyPage(int positionID)
    {
        // set real position on stream
        _contentStream!.Position = FILE_HEADER_SIZE + (positionID * PAGE_SIZE);

        _contentStream.Write(PAGE_EMPTY);
    }

    /// <summary>
    /// Write an empty (full \0) PAGE_SIZE using from/to (inclusive)
    /// </summary>
    public void WriteEmptyPages(int fromPositionID, int toPositionID, CancellationToken token = default)
    {
        for (var i = fromPositionID; i <= toPositionID && token.IsCancellationRequested; i++)
        {
            // set real position on stream
            _contentStream!.Position = FILE_HEADER_SIZE + (i * PAGE_SIZE);

            _contentStream.Write(PAGE_EMPTY);
        }
    }

    /// <summary>
    /// Set new file length using lastPageID as end of file.
    /// 0 = 8k, 1 = 16k, ...
    /// </summary>
    public void SetSize(int lastPageID)
    {
        var fileLength = FILE_HEADER_SIZE +
            ((lastPageID + 1) * PAGE_SIZE);

        _stream!.SetLength(fileLength);
    }

    /// <summary>
    /// Write a specific byte in datafile with a flag/byte value - used to restore. Use sync write
    /// </summary>
    public void WriteFlag(int headerPosition, byte flag)
    {
        _stream!.Position = FileHeader.P_IS_DIRTY;
        _stream.WriteByte(flag);

        _stream.Flush();
    }

    /// <summary>
    /// Close stream (disconect from disk)
    /// </summary>
    public void Dispose()
    {
        _stream?.Dispose();
        _contentStream?.Dispose();
    }
}
