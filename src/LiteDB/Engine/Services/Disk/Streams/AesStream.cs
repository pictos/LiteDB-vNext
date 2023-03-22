﻿namespace LiteDB.Engine;

/// <summary>
/// Encrypted AES Stream
/// </summary>
public class AesStream : Stream
{
    private readonly Aes _aes;
    private readonly ICryptoTransform _encryptor;
    private readonly ICryptoTransform _decryptor;

    private readonly Stream _stream;
    private readonly CryptoStream? _reader;
    private readonly CryptoStream? _writer;

    private readonly byte[] _decryptedZeroes = new byte[16];

    private static readonly byte[] _emptyContent = new byte[PAGE_SIZE - 1 - 16]; // 1 for aes indicator + 16 for salt 

    public override bool CanRead => _stream.CanRead;

    public override bool CanSeek => _stream.CanSeek;

    public override bool CanWrite => _stream.CanWrite;

    public override long Length => _stream.Length;

    public override long Position
    {
        get => _stream.Position;
        set => _stream.Position = value;
    }

    public AesStream(Stream stream, string password, byte[] salt)
    {
        _stream = stream;

        _aes = Aes.Create();
        _aes.Padding = PaddingMode.None;
        _aes.Mode = CipherMode.ECB;

        var pdb = new Rfc2898DeriveBytes(password, salt);

        using (pdb as IDisposable)
        {
            _aes.Key = pdb.GetBytes(32);
            _aes.IV = pdb.GetBytes(16);
        }

        _encryptor = _aes.CreateEncryptor();
        _decryptor = _aes.CreateDecryptor();

        _reader = _stream.CanRead ?
            new CryptoStream(_stream, _decryptor, CryptoStreamMode.Read) :
            null;

        _writer = _stream.CanWrite ?
            new CryptoStream(_stream, _encryptor, CryptoStreamMode.Write) :
            null;
    }

    /// <summary>
    /// Decrypt data from Stream. Must check if disk are not full 0 (skiped). In this case, must return empty
    /// </summary>
    public override int Read(byte[] array, int offset, int count)
    {
        var read = _reader!.Read(array, offset, count);

        return read;
    }

    /// <summary>
    /// Encrypt data to Stream
    /// </summary>
    public override void Write(byte[] array, int offset, int count)
    {
        _writer!.Write(array, offset, count);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _stream?.Dispose();

        _encryptor.Dispose();
        _decryptor.Dispose();

        _aes.Dispose();
    }

    /// <summary>
    /// Get new salt for encryption
    /// </summary>
    public static byte[] NewSalt()
    {
        var salt = new byte[ENCRYPTION_SALT_SIZE];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        return salt;
    }

    public override void Flush()
    {
        _stream.Flush();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _stream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _stream.SetLength(value);
    }

    private unsafe bool IsBlank(byte[] array, int offset)
    {
        fixed (byte* arrayPtr = array)
        fixed (void* vPtr = _decryptedZeroes)
        {
            ulong* ptr = (ulong*)(arrayPtr + offset);
            ulong* zeroptr = (ulong*)vPtr;

            return *ptr == *zeroptr && *(ptr + 1) == *(zeroptr + 1);
        }
    }
}