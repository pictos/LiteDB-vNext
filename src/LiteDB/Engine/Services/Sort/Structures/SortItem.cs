﻿namespace LiteDB.Engine;

internal struct SortItem
{
    public PageAddress RowID;
    public BsonValue Key;

    public SortItem(PageAddress rowID, BsonValue key)
    {
        this.RowID = rowID;
        this.Key = key;
    }

    public int GetBytesCount()
    {
        return IndexNode.GetKeyLength(this.Key) + PageAddress.SIZE;
    }
}