﻿namespace LiteDB;

/// <summary>
/// Represent an integer value in Bson object model
/// </summary>
public class BsonInt32 : BsonValue
{
    public int Value { get; }

    public BsonInt32(int value)
    {
        this.Value = value;
    }

    public override BsonType Type => BsonType.Int32;

    public override int GetBytesCount() => sizeof(int);

    public override int GetHashCode() => this.Value.GetHashCode();

    #region Implement CompareTo

    public override int CompareTo(BsonValue other, Collation collation)
    {
        if (other is BsonInt32 otherInt32) return this.Value.CompareTo(otherInt32.Value);
        if (other is BsonInt64 otherInt64) return this.ToInt64().CompareTo(otherInt64.Value);
        if (other is BsonDouble otherDouble) return this.ToDouble().CompareTo(otherDouble.Value);
        if (other is BsonDecimal otherDecimal) return this.ToDecimal().CompareTo(otherDecimal.Value);

        return this.CompareType(other);
    }

    #endregion

    #region Implicit Ctor

    public static implicit operator int(BsonInt32 value) => value.Value;

    public static implicit operator BsonInt32(int value) => new (value);

    #endregion

    #region Convert Types

    public override bool ToBoolean() => this.Value != 0;

    public override int ToInt32() => this.Value;

    public override long ToInt64() => this.Value;

    public override double ToDouble() => this.Value;

    public override decimal ToDecimal() => this.Value;

    public override string ToString() => this.Value.ToString(CultureInfo.InvariantCulture.NumberFormat);

    #endregion
}
