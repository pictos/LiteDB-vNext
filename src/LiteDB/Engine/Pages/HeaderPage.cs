namespace LiteDB.Engine;

/// <summary>
/// Header page represent first page on datafile. Engine contains a single instance of HeaderPage and all changes
/// must be syncornized (using lock).
/// </summary>
internal class HeaderPage : BasePage
{
    /// <summary>
    /// Header info the validate that datafile is a LiteDB file (27 bytes)
    /// </summary>
    public const string HEADER_INFO = "** This is a LiteDB file **";

    /// <summary>
    /// Datafile specification version
    /// </summary>
    public const byte FILE_VERSION = 9;

    #region Buffer Field Positions

    private const int P_CREATION_TIME = 5; // 5-13 (8 bytes)
    private const int P_LAST_PAGE_ID = 14; // 14-18 (4 bytes)

    public const int P_HEADER_INFO = 32;  // 32-58 (27 bytes)
    public const int P_FILE_VERSION = 59; // 59-59 (1 byte)

    #endregion

    /// <summary>
    /// DateTime when database was created [8 bytes]
    /// </summary>
    public DateTime CreationTime { get; } = DateTime.UtcNow;

    /// <summary>
    /// Get last physical page ID created [4 bytes]
    /// </summary>
    public uint LastPageID { get; private set; } = uint.MaxValue;

    /// <summary>
    /// Create new HeaderPage instance
    /// </summary>
    public HeaderPage(IMemoryOwner<byte> writeBuffer)
        : base(0, PageType.Header, writeBuffer)
    {
        var span = _writeBuffer!.Memory.Span;

        // update header
        span.WriteDateTime(this.CreationTime);

        // fixed content - can update buffer (header do not use shared cache)
        span[P_HEADER_INFO..].WriteString(HEADER_INFO);
        span[P_FILE_VERSION] = FILE_VERSION;
    }

    /// <summary>
    /// Load HeaderPage from buffer page
    /// </summary>
    public HeaderPage(IMemoryOwner<byte> buffer, IMemoryFactory memoryFactory)
        : base(buffer, memoryFactory)
    {
        var span = _readBuffer.Memory.Span;

        // read header
        this.CreationTime = span[P_CREATION_TIME..8].ReadDateTime();
        this.LastPageID = span[P_LAST_PAGE_ID..4].ReadUInt32();

        // read content: info and file version
        var info = span[P_HEADER_INFO..HEADER_INFO.Length].ReadString();
        var ver = span[P_FILE_VERSION];

        if (string.CompareOrdinal(info, HEADER_INFO) != 0 || ver != FILE_VERSION)
        {
            throw ERR_INVALID_DATABASE();
        }
    }

    public override IMemoryOwner<byte> GetBufferWrite()
    {
        var buffer = base.GetBufferWrite();
        var span = buffer.Memory.Span;

        //BinaryPrimitives.WriteUInt32LittleEndian(buffer.Memory.Span, this.LastPageID);

        var xx = buffer.Memory.Span[P_LAST_PAGE_ID..4];

        // update header
        //span[P_LAST_PAGE_ID..4].WriteUInt32(this.LastPageID);

        return buffer;
    }
}
