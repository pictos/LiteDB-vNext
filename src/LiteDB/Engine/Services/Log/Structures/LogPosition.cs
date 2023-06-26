﻿namespace LiteDB.Engine;

internal struct LogPosition : IEqualityComparer<LogPosition>
{
    public int PositionID;
    public int PageID;
    public int PhysicalID;
    public bool IsConfirmed;

    public bool Equals(LogPosition x, LogPosition y)
    {
        return x.PositionID == y.PositionID;
    }

    public int GetHashCode(LogPosition obj)
    {
        return obj.PositionID;
    }

    public override string ToString()
    {
        return $"PhysicalID: {PhysicalID}, PositionID: {PositionID}, PageID: {PageID} {(IsConfirmed ? "confirmed" : "not-confirmed")}";
    }
}