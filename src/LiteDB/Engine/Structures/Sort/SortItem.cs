﻿namespace LiteDB.Engine;

internal readonly struct SortItem : IIsEmpty
{
    public readonly RowID DataBlockID;
    public readonly BsonValue Key;

    public static readonly SortItem Empty = new();

    public bool IsEmpty => this.DataBlockID.IsEmpty;

    public SortItem()
    {
        this.DataBlockID = RowID.Empty;
        this.Key = BsonValue.MinValue;
    }

    public SortItem(RowID dataBlockID, BsonValue key)
    {
        this.DataBlockID = dataBlockID;
        this.Key = key;
    }

    public unsafe int GetBytesCount()
    {
        throw new NotImplementedException();
//        return IndexNode.GetKeyLength(this.Key) + sizeof(RowID);
    }

    public override string ToString() => Dump.Object(this);
}
