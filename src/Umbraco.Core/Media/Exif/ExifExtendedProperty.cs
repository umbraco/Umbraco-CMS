using System.Text;

namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     Represents an enumerated value.
/// </summary>
internal class ExifEnumProperty<T> : ExifProperty
    where T : notnull
{
    protected bool mIsBitField;
    protected T mValue;

    public ExifEnumProperty(ExifTag tag, T value, bool isbitfield)
        : base(tag)
    {
        mValue = value;
        mIsBitField = isbitfield;
    }

    public ExifEnumProperty(ExifTag tag, T value)
        : this(tag, value, false)
    {
    }

    public new T Value
    {
        get => mValue;
        set => mValue = value;
    }

    protected override object _Value
    {
        get => Value;
        set => Value = (T)value;
    }

    public bool IsBitField => mIsBitField;

    public override ExifInterOperability Interoperability
    {
        get
        {
            var tagid = ExifTagFactory.GetTagID(mTag);

            Type type = typeof(T);
            Type basetype = Enum.GetUnderlyingType(type);

            if (type == typeof(FileSource) || type == typeof(SceneType))
            {
                // UNDEFINED
                return new ExifInterOperability(tagid, 7, 1, new[] { (byte)(object)mValue });
            }

            if (type == typeof(GPSLatitudeRef) || type == typeof(GPSLongitudeRef) ||
                type == typeof(GPSStatus) || type == typeof(GPSMeasureMode) ||
                type == typeof(GPSSpeedRef) || type == typeof(GPSDirectionRef) ||
                type == typeof(GPSDistanceRef))
            {
                // ASCII
                return new ExifInterOperability(tagid, 2, 2, new byte[] { (byte)(object)mValue, 0 });
            }

            if (basetype == typeof(byte))
            {
                // BYTE
                return new ExifInterOperability(tagid, 1, 1, new[] { (byte)(object)mValue });
            }

            if (basetype == typeof(ushort))
            {
                // SHORT
                return new ExifInterOperability(
                    tagid,
                    3,
                    1,
                    BitConverterEx.GetBytes((ushort)(object)mValue, BitConverterEx.SystemByteOrder, BitConverterEx.SystemByteOrder));
            }

            throw new InvalidOperationException(
                $"An invalid enum type ({basetype.FullName}) was provided for type {type.FullName}");
        }
    }

    public static implicit operator T(ExifEnumProperty<T> obj) => obj.mValue;

    public override string? ToString() => mValue.ToString();
}

/// <summary>
///     Represents an ASCII string. (EXIF Specification: UNDEFINED) Used for the UserComment field.
/// </summary>
internal class ExifEncodedString : ExifProperty
{
    protected string mValue;

    public ExifEncodedString(ExifTag tag, string value, Encoding encoding)
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

    public Encoding Encoding { get; set; }

    public override ExifInterOperability Interoperability
    {
        get
        {
            var enc = string.Empty;
            if (Encoding == null)
            {
                enc = "\0\0\0\0\0\0\0\0";
            }
            else if (Encoding.EncodingName == "US-ASCII")
            {
                enc = "ASCII\0\0\0";
            }
            else if (Encoding.EncodingName == "Japanese (JIS 0208-1990 and 0212-1990)")
            {
                enc = "JIS\0\0\0\0\0";
            }
            else if (Encoding.EncodingName == "Unicode")
            {
                enc = "Unicode\0";
            }
            else
            {
                enc = "\0\0\0\0\0\0\0\0";
            }

            var benc = Encoding.ASCII.GetBytes(enc);
            var bstr = Encoding == null ? Encoding.ASCII.GetBytes(mValue) : Encoding.GetBytes(mValue);
            var data = new byte[benc.Length + bstr.Length];
            Array.Copy(benc, 0, data, 0, benc.Length);
            Array.Copy(bstr, 0, data, benc.Length, bstr.Length);

            return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 7, (uint)data.Length, data);
        }
    }

    public static implicit operator string(ExifEncodedString obj) => obj.mValue;

    public override string ToString() => mValue;
}

/// <summary>
///     Represents an ASCII string formatted as DateTime. (EXIF Specification: ASCII) Used for the date time fields.
/// </summary>
internal class ExifDateTime : ExifProperty
{
    protected DateTime mValue;

    public ExifDateTime(ExifTag tag, DateTime value)
        : base(tag) =>
        mValue = value;

    public new DateTime Value
    {
        get => mValue;
        set => mValue = value;
    }

    protected override object _Value
    {
        get => Value;
        set => Value = (DateTime)value;
    }

    public override ExifInterOperability Interoperability =>
        new(ExifTagFactory.GetTagID(mTag), 2, 20, ExifBitConverter.GetBytes(mValue, true));

    public static implicit operator DateTime(ExifDateTime obj) => obj.mValue;

    public override string ToString() => mValue.ToString("yyyy.MM.dd HH:mm:ss");
}

/// <summary>
///     Represents the exif version as a 4 byte ASCII string. (EXIF Specification: UNDEFINED)
///     Used for the ExifVersion, FlashpixVersion, InteroperabilityVersion and GPSVersionID fields.
/// </summary>
internal class ExifVersion : ExifProperty
{
    protected string mValue;

    public ExifVersion(ExifTag tag, string value)
        : base(tag)
    {
        if (value.Length > 4)
        {
            mValue = value[..4];
        }
        else if (value.Length < 4)
        {
            mValue = value + new string(' ', 4 - value.Length);
        }
        else
        {
            mValue = value;
        }
    }

    public new string Value
    {
        get => mValue;
        set => mValue = value[..4];
    }

    protected override object _Value
    {
        get => Value;
        set => Value = (string)value;
    }

    public override ExifInterOperability Interoperability
    {
        get
        {
            if (mTag == ExifTag.ExifVersion || mTag == ExifTag.FlashpixVersion ||
                mTag == ExifTag.InteroperabilityVersion)
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 7, 4, Encoding.ASCII.GetBytes(mValue));
            }

            var data = new byte[4];
            for (var i = 0; i < 4; i++)
            {
                data[i] = byte.Parse(mValue[0].ToString());
            }

            return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 7, 4, data);
        }
    }

    public override string ToString() => mValue;
}

/// <summary>
///     Represents the location and area of the subject (EXIF Specification: 2xSHORT)
///     The coordinate values, width, and height are expressed in relation to the
///     upper left as origin, prior to rotation processing as per the Rotation tag.
/// </summary>
internal class ExifPointSubjectArea : ExifUShortArray
{
    public ExifPointSubjectArea(ExifTag tag, ushort[] value)
        : base(tag, value)
    {
    }

    public ExifPointSubjectArea(ExifTag tag, ushort x, ushort y)
        : base(tag, new[] { x, y })
    {
    }

    public ushort X
    {
        get => mValue[0];
        set => mValue[0] = value;
    }

    protected new ushort[] Value
    {
        get => mValue;
        set => mValue = value;
    }

    public ushort Y
    {
        get => mValue[1];
        set => mValue[1] = value;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendFormat("({0:d}, {1:d})", mValue[0], mValue[1]);
        return sb.ToString();
    }
}

/// <summary>
///     Represents the location and area of the subject (EXIF Specification: 3xSHORT)
///     The coordinate values, width, and height are expressed in relation to the
///     upper left as origin, prior to rotation processing as per the Rotation tag.
/// </summary>
internal class ExifCircularSubjectArea : ExifPointSubjectArea
{
    public ExifCircularSubjectArea(ExifTag tag, ushort[] value)
        : base(tag, value)
    {
    }

    public ExifCircularSubjectArea(ExifTag tag, ushort x, ushort y, ushort d)
        : base(tag, new[] { x, y, d })
    {
    }

    public ushort Diamater
    {
        get => mValue[2];
        set => mValue[2] = value;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendFormat("({0:d}, {1:d}) {2:d}", mValue[0], mValue[1], mValue[2]);
        return sb.ToString();
    }
}

/// <summary>
///     Represents the location and area of the subject (EXIF Specification: 4xSHORT)
///     The coordinate values, width, and height are expressed in relation to the
///     upper left as origin, prior to rotation processing as per the Rotation tag.
/// </summary>
internal class ExifRectangularSubjectArea : ExifPointSubjectArea
{
    public ExifRectangularSubjectArea(ExifTag tag, ushort[] value)
        : base(tag, value)
    {
    }

    public ExifRectangularSubjectArea(ExifTag tag, ushort x, ushort y, ushort w, ushort h)
        : base(tag, new[] { x, y, w, h })
    {
    }

    public ushort Width
    {
        get => mValue[2];
        set => mValue[2] = value;
    }

    public ushort Height
    {
        get => mValue[3];
        set => mValue[3] = value;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendFormat("({0:d}, {1:d}) ({2:d} x {3:d})", mValue[0], mValue[1], mValue[2], mValue[3]);
        return sb.ToString();
    }
}

/// <summary>
///     Represents GPS latitudes and longitudes (EXIF Specification: 3xRATIONAL)
/// </summary>
internal class GPSLatitudeLongitude : ExifURationalArray
{
    public GPSLatitudeLongitude(ExifTag tag, MathEx.UFraction32[] value)
        : base(tag, value)
    {
    }

    public GPSLatitudeLongitude(ExifTag tag, float d, float m, float s)
        : base(tag, new[] { new(d), new MathEx.UFraction32(m), new MathEx.UFraction32(s) })
    {
    }

    public MathEx.UFraction32 Degrees
    {
        get => mValue[0];
        set => mValue[0] = value;
    }

    protected new MathEx.UFraction32[] Value
    {
        get => mValue;
        set => mValue = value;
    }

    public MathEx.UFraction32 Minutes
    {
        get => mValue[1];
        set => mValue[1] = value;
    }

    public MathEx.UFraction32 Seconds
    {
        get => mValue[2];
        set => mValue[2] = value;
    }

    public static explicit operator float(GPSLatitudeLongitude obj) => obj.ToFloat();

    public float ToFloat() => (float)Degrees + ((float)Minutes / 60.0f) + ((float)Seconds / 3600.0f);

    public override string ToString() =>
        string.Format("{0:F2}°{1:F2}'{2:F2}\"", (float)Degrees, (float)Minutes, (float)Seconds);
}

/// <summary>
///     Represents a GPS time stamp as UTC (EXIF Specification: 3xRATIONAL)
/// </summary>
internal class GPSTimeStamp : ExifURationalArray
{
    public GPSTimeStamp(ExifTag tag, MathEx.UFraction32[] value)
        : base(tag, value)
    {
    }

    public GPSTimeStamp(ExifTag tag, float h, float m, float s)
        : base(tag, new[] { new(h), new MathEx.UFraction32(m), new MathEx.UFraction32(s) })
    {
    }

    public MathEx.UFraction32 Hour
    {
        get => mValue[0];
        set => mValue[0] = value;
    }

    protected new MathEx.UFraction32[] Value
    {
        get => mValue;
        set => mValue = value;
    }

    public MathEx.UFraction32 Minute
    {
        get => mValue[1];
        set => mValue[1] = value;
    }

    public MathEx.UFraction32 Second
    {
        get => mValue[2];
        set => mValue[2] = value;
    }

    public override string ToString() =>
        string.Format("{0:F2}:{1:F2}:{2:F2}\"", (float)Hour, (float)Minute, (float)Second);
}

/// <summary>
///     Represents an ASCII string. (EXIF Specification: BYTE)
///     Used by Windows XP.
/// </summary>
internal class WindowsByteString : ExifProperty
{
    protected string mValue;

    public WindowsByteString(ExifTag tag, string value)
        : base(tag) =>
        mValue = value;

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

    public override ExifInterOperability Interoperability
    {
        get
        {
            var data = Encoding.Unicode.GetBytes(mValue);
            return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 1, (uint)data.Length, data);
        }
    }

    public static implicit operator string(WindowsByteString obj) => obj.mValue;

    public override string ToString() => mValue;
}
