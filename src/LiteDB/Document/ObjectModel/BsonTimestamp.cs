﻿namespace LiteDB;

/// <summary>
/// Represent a timestamp value (UInt64) in Bson object model
/// </summary>
public class BsonTimestamp : BsonValue, IComparable<BsonTimestamp>, IEquatable<BsonTimestamp>
{
    public UInt64 Value { get; }

    public BsonTimestamp(UInt64 value)
    {
        this.Value = value;
    }

    public override BsonType Type => BsonType.UInt64;

    public override int GetBytesCount() => sizeof(UInt64);

    #region Implement IComparable and IEquatable

    public override int CompareTo(BsonValue other, Collation collation)
    {
        if (other == null) return 1;

        if (other is BsonTimestamp otherUInt64) return this.Value.CompareTo(otherUInt64.Value);
        if (other is BsonInt32 otherInt32) return this.Value.CompareTo(otherInt32.Value);
        if (other is BsonInt64 otherInt64) return this.Value.CompareTo(otherInt64.Value);
        if (other is BsonDouble otherDouble) return this.Value.CompareTo(otherDouble.Value);
        if (other is BsonDecimal otherDecimal) return this.Value.CompareTo(otherDecimal.Value);

        return this.CompareType(other);
    }

    public int CompareTo(BsonTimestamp other)
    {
        if (other == null) return 1;

        return this.Value.CompareTo(other.Value);
    }

    public bool Equals(BsonTimestamp other)
    {
        if (other is null) return false;

        return this.Value == other.Value;
    }

    #endregion

    #region Explicit operators

    public static bool operator ==(BsonTimestamp left, BsonTimestamp right) => left.Equals(right);

    public static bool operator !=(BsonTimestamp left, BsonTimestamp right) => !left.Equals(right);

    #endregion

    #region Implicit Ctor

    public static implicit operator UInt64(BsonTimestamp value) => value.Value;

    public static implicit operator BsonTimestamp(UInt64 value) => new BsonTimestamp(value);

    #endregion

    #region GetHashCode, Equals, ToString override

    public override int GetHashCode() => this.Value.GetHashCode();

    public override bool Equals(object other) => this.Value.Equals(other);

    public override string ToString() => this.Value.ToString();

    #endregion
}
