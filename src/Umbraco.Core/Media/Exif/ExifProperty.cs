using System.Text;

namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     Represents the abstract base class for an Exif property.
/// </summary>
internal abstract class ExifProperty
{
    protected IFD mIFD;
    protected string? mName;
    protected ExifTag mTag;

    public ExifProperty(ExifTag tag)
    {
        mTag = tag;
        mIFD = ExifTagFactory.GetTagIFD(tag);
    }

    /// <summary>
    ///     Gets the Exif tag associated with this property.
    /// </summary>
    public ExifTag Tag => mTag;

    /// <summary>
    ///     Gets the IFD section containing this property.
    /// </summary>
    public IFD IFD => mIFD;

    /// <summary>
    ///     Gets or sets the name of this property.
    /// </summary>
    public string Name
    {
        get
        {
            if (string.IsNullOrEmpty(mName))
            {
                return ExifTagFactory.GetTagName(mTag);
            }

            return mName;
        }
        set => mName = value;
    }

    /// <summary>
    ///     Gets or sets the value of this property.
    /// </summary>
    public object Value
    {
        get => _Value;
        set => _Value = value;
    }

    protected abstract object _Value { get; set; }

    /// <summary>
    ///     Gets interoperability data for this property.
    /// </summary>
    public abstract ExifInterOperability Interoperability { get; }
}

/// <summary>
///     Represents an 8-bit unsigned integer. (EXIF Specification: BYTE)
/// </summary>
internal class ExifByte : ExifProperty
{
    protected byte mValue;

    public ExifByte(ExifTag tag, byte value)
        : base(tag) =>
        mValue = value;

    public new byte Value
    {
        get => mValue;
        set => mValue = value;
    }

    protected override object _Value
    {
        get => Value;
        set => Value = Convert.ToByte(value);
    }

    public override ExifInterOperability Interoperability =>
        new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 1, 1, new[] { mValue });

    public static implicit operator byte(ExifByte obj) => obj.mValue;

    public override string ToString() => mValue.ToString();
}

/// <summary>
///     Represents an array of 8-bit unsigned integers. (EXIF Specification: BYTE with count > 1)
/// </summary>
internal class ExifByteArray : ExifProperty
{
    protected byte[] mValue;

    public ExifByteArray(ExifTag tag, byte[] value)
        : base(tag) =>
        mValue = value;

    public new byte[] Value
    {
        get => mValue;
        set => mValue = value;
    }

    protected override object _Value
    {
        get => Value;
        set => Value = (byte[])value;
    }

    public override ExifInterOperability Interoperability =>
        new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 1, (uint)mValue.Length, mValue);

    public static implicit operator byte[](ExifByteArray obj) => obj.mValue;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append('[');
        foreach (var b in mValue)
        {
            sb.Append(b);
            sb.Append(' ');
        }

        sb.Remove(sb.Length - 1, 1);
        sb.Append(']');
        return sb.ToString();
    }
}

/// <summary>
///     Represents an ASCII string. (EXIF Specification: ASCII)
/// </summary>
internal class ExifAscii : ExifProperty
{
    protected string mValue;

    public ExifAscii(ExifTag tag, string value, Encoding encoding)
        : base(tag)
    {
        mValue = value;
        Encoding = encoding;
    }

    public new string Value
    {
        get => mValue;
        set => mValue = value;
    }

    protected override object _Value
    {
        get => Value;
        set => Value = (string)value;
    }

    public Encoding Encoding { get; }

    public override ExifInterOperability Interoperability =>
        new ExifInterOperability(
            ExifTagFactory.GetTagID(mTag),
            2,
            (uint)mValue.Length + 1,
            ExifBitConverter.GetBytes(mValue, true, Encoding));

    public static implicit operator string(ExifAscii obj) => obj.mValue;

    public override string ToString() => mValue;
}

/// <summary>
///     Represents a 16-bit unsigned integer. (EXIF Specification: SHORT)
/// </summary>
internal class ExifUShort : ExifProperty
{
    protected ushort mValue;

    public ExifUShort(ExifTag tag, ushort value)
        : base(tag) =>
        mValue = value;

    public new ushort Value
    {
        get => mValue;
        set => mValue = value;
    }

    protected override object _Value
    {
        get => Value;
        set => Value = Convert.ToUInt16(value);
    }

    public override ExifInterOperability Interoperability =>
        new ExifInterOperability(
            ExifTagFactory.GetTagID(mTag),
            3,
            1,
            BitConverterEx.GetBytes(mValue, BitConverterEx.SystemByteOrder, BitConverterEx.SystemByteOrder));

    public static implicit operator ushort(ExifUShort obj) => obj.mValue;

    public override string ToString() => mValue.ToString();
}

/// <summary>
///     Represents an array of 16-bit unsigned integers.
///     (EXIF Specification: SHORT with count > 1)
/// </summary>
internal class ExifUShortArray : ExifProperty
{
    protected ushort[] mValue;

    public ExifUShortArray(ExifTag tag, ushort[] value)
        : base(tag) =>
        mValue = value;

    public new ushort[] Value
    {
        get => mValue;
        set => mValue = value;
    }

    protected override object _Value
    {
        get => Value;
        set => Value = (ushort[])value;
    }

    public override ExifInterOperability Interoperability =>
        new ExifInterOperability(
            ExifTagFactory.GetTagID(mTag),
            3,
            (uint)mValue.Length,
            ExifBitConverter.GetBytes(mValue, BitConverterEx.SystemByteOrder));

    public static implicit operator ushort[](ExifUShortArray obj) => obj.mValue;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append('[');
        foreach (var b in mValue)
        {
            sb.Append(b);
            sb.Append(' ');
        }

        sb.Remove(sb.Length - 1, 1);
        sb.Append(']');
        return sb.ToString();
    }
}

/// <summary>
///     Represents a 32-bit unsigned integer. (EXIF Specification: LONG)
/// </summary>
internal class ExifUInt : ExifProperty
{
    protected uint mValue;

    public ExifUInt(ExifTag tag, uint value)
        : base(tag) =>
        mValue = value;

    public new uint Value
    {
        get => mValue;
        set => mValue = value;
    }

    protected override object _Value
    {
        get => Value;
        set => Value = Convert.ToUInt32(value);
    }

    public override ExifInterOperability Interoperability =>
        new ExifInterOperability(
            ExifTagFactory.GetTagID(mTag),
            4,
            1,
            BitConverterEx.GetBytes(mValue, BitConverterEx.SystemByteOrder, BitConverterEx.SystemByteOrder));

    public static implicit operator uint(ExifUInt obj) => obj.mValue;

    public override string ToString() => mValue.ToString();
}

/// <summary>
///     Represents an array of 16-bit unsigned integers.
///     (EXIF Specification: LONG with count > 1)
/// </summary>
internal class ExifUIntArray : ExifProperty
{
    protected uint[] mValue;

    public ExifUIntArray(ExifTag tag, uint[] value)
        : base(tag) =>
        mValue = value;

    public new uint[] Value
    {
        get => mValue;
        set => mValue = value;
    }

    protected override object _Value
    {
        get => Value;
        set => Value = (uint[])value;
    }

    public override ExifInterOperability Interoperability => new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 3, (uint)mValue.Length, ExifBitConverter.GetBytes(mValue, BitConverterEx.SystemByteOrder));

    public static implicit operator uint[](ExifUIntArray obj) => obj.mValue;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append('[');
        foreach (var b in mValue)
        {
            sb.Append(b);
            sb.Append(' ');
        }

        sb.Remove(sb.Length - 1, 1);
        sb.Append(']');
        return sb.ToString();
    }
}

/// <summary>
///     Represents a rational number defined with a 32-bit unsigned numerator
///     and denominator. (EXIF Specification: RATIONAL)
/// </summary>
internal class ExifURational : ExifProperty
{
    protected MathEx.UFraction32 mValue;

    public ExifURational(ExifTag tag, uint numerator, uint denominator)
        : base(tag) =>
        mValue = new MathEx.UFraction32(numerator, denominator);

    public ExifURational(ExifTag tag, MathEx.UFraction32 value)
        : base(tag) =>
        mValue = value;

    public new MathEx.UFraction32 Value
    {
        get => mValue;
        set => mValue = value;
    }

    protected override object _Value
    {
        get => Value;
        set => Value = (MathEx.UFraction32)value;
    }

    public override ExifInterOperability Interoperability =>
        new ExifInterOperability(
            ExifTagFactory.GetTagID(mTag),
            5,
            1,
            ExifBitConverter.GetBytes(mValue, BitConverterEx.SystemByteOrder));

    public static explicit operator float(ExifURational obj) => (float)obj.mValue;

    public override string ToString() => mValue.ToString();

    public float ToFloat() => (float)mValue;

    public uint[] ToArray() => new[] { mValue.Numerator, mValue.Denominator };
}

/// <summary>
///     Represents an array of unsigned rational numbers.
///     (EXIF Specification: RATIONAL with count > 1)
/// </summary>
internal class ExifURationalArray : ExifProperty
{
    protected MathEx.UFraction32[] mValue;

    public ExifURationalArray(ExifTag tag, MathEx.UFraction32[] value)
        : base(tag) =>
        mValue = value;

    public new MathEx.UFraction32[] Value
    {
        get => mValue;
        set => mValue = value;
    }

    protected override object _Value
    {
        get => Value;
        set => Value = (MathEx.UFraction32[])value;
    }

    public override ExifInterOperability Interoperability =>
        new ExifInterOperability(
            ExifTagFactory.GetTagID(mTag),
            5,
            (uint)mValue.Length,
            ExifBitConverter.GetBytes(mValue, BitConverterEx.SystemByteOrder));

    public static explicit operator float[](ExifURationalArray obj)
    {
        var result = new float[obj.mValue.Length];
        for (var i = 0; i < obj.mValue.Length; i++)
        {
            result[i] = (float)obj.mValue[i];
        }

        return result;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append('[');
        foreach (MathEx.UFraction32 b in mValue)
        {
            sb.Append(b.ToString());
            sb.Append(' ');
        }

        sb.Remove(sb.Length - 1, 1);
        sb.Append(']');
        return sb.ToString();
    }
}

/// <summary>
///     Represents a byte array that can take any value. (EXIF Specification: UNDEFINED)
/// </summary>
internal class ExifUndefined : ExifProperty
{
    protected byte[] mValue;

    public ExifUndefined(ExifTag tag, byte[] value)
        : base(tag) =>
        mValue = value;

    public new byte[] Value
    {
        get => mValue;
        set => mValue = value;
    }

    protected override object _Value
    {
        get => Value;
        set => Value = (byte[])value;
    }

    public override ExifInterOperability Interoperability =>
        new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 7, (uint)mValue.Length, mValue);

    public static implicit operator byte[](ExifUndefined obj) => obj.mValue;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append('[');
        foreach (var b in mValue)
        {
            sb.Append(b);
            sb.Append(' ');
        }

        sb.Remove(sb.Length - 1, 1);
        sb.Append(']');
        return sb.ToString();
    }
}

/// <summary>
///     Represents a 32-bit signed integer. (EXIF Specification: SLONG)
/// </summary>
internal class ExifSInt : ExifProperty
{
    protected int mValue;

    public ExifSInt(ExifTag tag, int value)
        : base(tag) =>
        mValue = value;

    public new int Value
    {
        get => mValue;
        set => mValue = value;
    }

    protected override object _Value
    {
        get => Value;
        set => Value = Convert.ToInt32(value);
    }

    public override ExifInterOperability Interoperability =>
        new ExifInterOperability(
            ExifTagFactory.GetTagID(mTag),
            9,
            1,
            BitConverterEx.GetBytes(mValue, BitConverterEx.SystemByteOrder, BitConverterEx.SystemByteOrder));

    public static implicit operator int(ExifSInt obj) => obj.mValue;

    public override string ToString() => mValue.ToString();
}

/// <summary>
///     Represents an array of 32-bit signed integers.
///     (EXIF Specification: SLONG with count > 1)
/// </summary>
internal class ExifSIntArray : ExifProperty
{
    protected int[] mValue;

    public ExifSIntArray(ExifTag tag, int[] value)
        : base(tag) =>
        mValue = value;

    public new int[] Value
    {
        get => mValue;
        set => mValue = value;
    }

    protected override object _Value
    {
        get => Value;
        set => Value = (int[])value;
    }

    public override ExifInterOperability Interoperability =>
        new ExifInterOperability(
            ExifTagFactory.GetTagID(mTag),
            9,
            (uint)mValue.Length,
            ExifBitConverter.GetBytes(mValue, BitConverterEx.SystemByteOrder));

    public static implicit operator int[](ExifSIntArray obj) => obj.mValue;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append('[');
        foreach (var b in mValue)
        {
            sb.Append(b);
            sb.Append(' ');
        }

        sb.Remove(sb.Length - 1, 1);
        sb.Append(']');
        return sb.ToString();
    }
}

/// <summary>
///     Represents a rational number defined with a 32-bit signed numerator
///     and denominator. (EXIF Specification: SRATIONAL)
/// </summary>
internal class ExifSRational : ExifProperty
{
    protected MathEx.Fraction32 mValue;

    public ExifSRational(ExifTag tag, int numerator, int denominator)
        : base(tag) =>
        mValue = new MathEx.Fraction32(numerator, denominator);

    public ExifSRational(ExifTag tag, MathEx.Fraction32 value)
        : base(tag) =>
        mValue = value;

    public new MathEx.Fraction32 Value
    {
        get => mValue;
        set => mValue = value;
    }

    protected override object _Value
    {
        get => Value;
        set => Value = (MathEx.Fraction32)value;
    }

    public override ExifInterOperability Interoperability =>
        new ExifInterOperability(
            ExifTagFactory.GetTagID(mTag),
            10,
            1,
            ExifBitConverter.GetBytes(mValue, BitConverterEx.SystemByteOrder));

    public static explicit operator float(ExifSRational obj) => (float)obj.mValue;

    public override string ToString() => mValue.ToString();

    public float ToFloat() => (float)mValue;

    public int[] ToArray() => new[] { mValue.Numerator, mValue.Denominator };
}

/// <summary>
///     Represents an array of signed rational numbers.
///     (EXIF Specification: SRATIONAL with count > 1)
/// </summary>
internal class ExifSRationalArray : ExifProperty
{
    protected MathEx.Fraction32[] mValue;

    public ExifSRationalArray(ExifTag tag, MathEx.Fraction32[] value)
        : base(tag) =>
        mValue = value;

    public new MathEx.Fraction32[] Value
    {
        get => mValue;
        set => mValue = value;
    }

    protected override object _Value
    {
        get => Value;
        set => Value = (MathEx.Fraction32[])value;
    }

    public override ExifInterOperability Interoperability =>
        new ExifInterOperability(
            ExifTagFactory.GetTagID(mTag),
            10,
            (uint)mValue.Length,
            ExifBitConverter.GetBytes(mValue, BitConverterEx.SystemByteOrder));

    public static explicit operator float[](ExifSRationalArray obj)
    {
        var result = new float[obj.mValue.Length];
        for (var i = 0; i < obj.mValue.Length; i++)
        {
            result[i] = (float)obj.mValue[i];
        }

        return result;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append('[');
        foreach (MathEx.Fraction32 b in mValue)
        {
            sb.Append(b.ToString());
            sb.Append(' ');
        }

        sb.Remove(sb.Length - 1, 1);
        sb.Append(']');
        return sb.ToString();
    }
}
