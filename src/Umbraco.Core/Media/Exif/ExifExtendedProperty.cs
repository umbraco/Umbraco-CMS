using System;
using System.Text;

namespace Umbraco.Core.Media.Exif
{
    /// <summary>
    /// Represents an enumerated value.
    /// </summary>
    internal class ExifEnumProperty<T> : ExifProperty
    {
        protected T mValue;
        protected bool mIsBitField;
        protected override object _Value { get { return Value; } set { Value = (T)value; } }
        public new T Value { get { return mValue; } set { mValue = value; } }
        public bool IsBitField { get { return mIsBitField; } }

        static public implicit operator T(ExifEnumProperty<T> obj) { return (T)obj.mValue; }

        public override string ToString() { return mValue.ToString(); }

        public ExifEnumProperty(ExifTag tag, T value, bool isbitfield)
            : base(tag)
        {
            mValue = value;
            mIsBitField = isbitfield;
        }

        public ExifEnumProperty(ExifTag tag, T value)
            : this(tag, value, false)
        {
            ;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                ushort tagid = ExifTagFactory.GetTagID(mTag);

                Type type = typeof(T);
                Type basetype = Enum.GetUnderlyingType(type);

                if (type == typeof(FileSource) || type == typeof(SceneType))
                {
                    // UNDEFINED
                    return new ExifInterOperability(tagid, 7, 1, new byte[] { (byte)((object)mValue) });
                }
                else if (type == typeof(GPSLatitudeRef) || type == typeof(GPSLongitudeRef) ||
                    type == typeof(GPSStatus) || type == typeof(GPSMeasureMode) ||
                    type == typeof(GPSSpeedRef) || type == typeof(GPSDirectionRef) ||
                    type == typeof(GPSDistanceRef))
                {
                    // ASCII
                    return new ExifInterOperability(tagid, 2, 2, new byte[] { (byte)((object)mValue), 0 });
                }
                else if (basetype == typeof(byte))
                {
                    // BYTE
                    return new ExifInterOperability(tagid, 1, 1, new byte[] { (byte)((object)mValue) });
                }
                else if (basetype == typeof(ushort))
                {
                    // SHORT
                    return new ExifInterOperability(tagid, 3, 1, ExifBitConverter.GetBytes((ushort)((object)mValue), BitConverterEx.SystemByteOrder, BitConverterEx.SystemByteOrder));
                }
                else
                    throw new UnknownEnumTypeException();
            }
        }
    }

    /// <summary>
    /// Represents an ASCII string. (EXIF Specification: UNDEFINED) Used for the UserComment field.
    /// </summary>
    internal class ExifEncodedString : ExifProperty
    {
        protected string mValue;
        private Encoding mEncoding;
        protected override object _Value { get { return Value; } set { Value = (string)value; } }
        public new string Value { get { return mValue; } set { mValue = value; } }
        public Encoding Encoding { get { return mEncoding; } set { mEncoding = value; } }

        static public implicit operator string(ExifEncodedString obj) { return obj.mValue; }

        public override string ToString() { return mValue; }

        public ExifEncodedString(ExifTag tag, string value, Encoding encoding)
            : base(tag)
        {
            mValue = value;
            mEncoding = encoding;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                string enc = "";
                if (mEncoding == null)
                    enc = "\0\0\0\0\0\0\0\0";
                else if (mEncoding.EncodingName == "US-ASCII")
                    enc = "ASCII\0\0\0";
                else if (mEncoding.EncodingName == "Japanese (JIS 0208-1990 and 0212-1990)")
                    enc = "JIS\0\0\0\0\0";
                else if (mEncoding.EncodingName == "Unicode")
                    enc = "Unicode\0";
                else
                    enc = "\0\0\0\0\0\0\0\0";

                byte[] benc = Encoding.ASCII.GetBytes(enc);
                byte[] bstr = (mEncoding == null ? Encoding.ASCII.GetBytes(mValue) : mEncoding.GetBytes(mValue));
                byte[] data = new byte[benc.Length + bstr.Length];
                Array.Copy(benc, 0, data, 0, benc.Length);
                Array.Copy(bstr, 0, data, benc.Length, bstr.Length);

                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 7, (uint)data.Length, data);
            }
        }
    }

    /// <summary>
    /// Represents an ASCII string formatted as DateTime. (EXIF Specification: ASCII) Used for the date time fields.
    /// </summary>
    internal class ExifDateTime : ExifProperty
    {
        protected DateTime mValue;
        protected override object _Value { get { return Value; } set { Value = (DateTime)value; } }
        public new DateTime Value { get { return mValue; } set { mValue = value; } }

        static public implicit operator DateTime(ExifDateTime obj) { return obj.mValue; }

        public override string ToString() { return mValue.ToString("yyyy.MM.dd HH:mm:ss"); }

        public ExifDateTime(ExifTag tag, DateTime value)
            : base(tag)
        {
            mValue = value;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 2, (uint)20, ExifBitConverter.GetBytes(mValue, true));
            }
        }
    }

    /// <summary>
    /// Represents the exif version as a 4 byte ASCII string. (EXIF Specification: UNDEFINED) 
    /// Used for the ExifVersion, FlashpixVersion, InteroperabilityVersion and GPSVersionID fields.
    /// </summary>
    internal class ExifVersion : ExifProperty
    {
        protected string mValue;
        protected override object _Value { get { return Value; } set { Value = (string)value; } }
        public new string Value { get { return mValue; } set { mValue = value.Substring(0, 4); } }

        public ExifVersion(ExifTag tag, string value)
            : base(tag)
        {
            if (value.Length > 4)
                mValue = value.Substring(0, 4);
            else if (value.Length < 4)
                mValue = value + new string(' ', 4 - value.Length);
            else
                mValue = value;
        }

        public override string ToString()
        {
            return mValue;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                if (mTag == ExifTag.ExifVersion || mTag == ExifTag.FlashpixVersion || mTag == ExifTag.InteroperabilityVersion)
                    return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 7, 4, Encoding.ASCII.GetBytes(mValue));
                else
                {
                    byte[] data = new byte[4];
                    for (int i = 0; i < 4; i++)
                        data[i] = byte.Parse(mValue[0].ToString());
                    return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 7, 4, data);
                }
            }
        }
    }

    /// <summary>
    /// Represents the location and area of the subject (EXIF Specification: 2xSHORT)
    /// The coordinate values, width, and height are expressed in relation to the 
    /// upper left as origin, prior to rotation processing as per the Rotation tag.
    /// </summary>
    internal class ExifPointSubjectArea : ExifUShortArray
    {
        protected new ushort[] Value { get { return mValue; } set { mValue = value; } }
        public ushort X { get { return mValue[0]; } set { mValue[0] = value; } }
        public ushort Y { get { return mValue[1]; } set { mValue[1] = value; } }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("({0:d}, {1:d})", mValue[0], mValue[1]);
            return sb.ToString();
        }

        public ExifPointSubjectArea(ExifTag tag, ushort[] value)
            : base(tag, value)
        {
            ;
        }

        public ExifPointSubjectArea(ExifTag tag, ushort x, ushort y)
            : base(tag, new ushort[] { x, y })
        {
            ;
        }
    }

    /// <summary>
    /// Represents the location and area of the subject (EXIF Specification: 3xSHORT)
    /// The coordinate values, width, and height are expressed in relation to the 
    /// upper left as origin, prior to rotation processing as per the Rotation tag.
    /// </summary>
    internal class ExifCircularSubjectArea : ExifPointSubjectArea
    {
        public ushort Diamater { get { return mValue[2]; } set { mValue[2] = value; } }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("({0:d}, {1:d}) {2:d}", mValue[0], mValue[1], mValue[2]);
            return sb.ToString();
        }

        public ExifCircularSubjectArea(ExifTag tag, ushort[] value)
            : base(tag, value)
        {
            ;
        }

        public ExifCircularSubjectArea(ExifTag tag, ushort x, ushort y, ushort d)
            : base(tag, new ushort[] { x, y, d })
        {
            ;
        }
    }

    /// <summary>
    /// Represents the location and area of the subject (EXIF Specification: 4xSHORT)
    /// The coordinate values, width, and height are expressed in relation to the 
    /// upper left as origin, prior to rotation processing as per the Rotation tag.
    /// </summary>
    internal class ExifRectangularSubjectArea : ExifPointSubjectArea
    {
        public ushort Width { get { return mValue[2]; } set { mValue[2] = value; } }
        public ushort Height { get { return mValue[3]; } set { mValue[3] = value; } }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("({0:d}, {1:d}) ({2:d} x {3:d})", mValue[0], mValue[1], mValue[2], mValue[3]);
            return sb.ToString();
        }

        public ExifRectangularSubjectArea(ExifTag tag, ushort[] value)
            : base(tag, value)
        {
            ;
        }

        public ExifRectangularSubjectArea(ExifTag tag, ushort x, ushort y, ushort w, ushort h)
            : base(tag, new ushort[] { x, y, w, h })
        {
            ;
        }
    }

    /// <summary>
    /// Represents GPS latitudes and longitudes (EXIF Specification: 3xRATIONAL)
    /// </summary>
    internal class GPSLatitudeLongitude : ExifURationalArray
    {
        protected new MathEx.UFraction32[] Value { get { return mValue; } set { mValue = value; } }
        public MathEx.UFraction32 Degrees { get { return mValue[0]; } set { mValue[0] = value; } }
        public MathEx.UFraction32 Minutes { get { return mValue[1]; } set { mValue[1] = value; } }
        public MathEx.UFraction32 Seconds { get { return mValue[2]; } set { mValue[2] = value; } }

        public static explicit operator float(GPSLatitudeLongitude obj) { return obj.ToFloat(); }
        public float ToFloat()
        {
            return (float)Degrees + ((float)Minutes) / 60.0f + ((float)Seconds) / 3600.0f;
        }

        public override string ToString()
        {
            return string.Format("{0:F2}°{1:F2}'{2:F2}\"", (float)Degrees, (float)Minutes, (float)Seconds);
        }

        public GPSLatitudeLongitude(ExifTag tag, MathEx.UFraction32[] value)
            : base(tag, value)
        {
            ;
        }

        public GPSLatitudeLongitude(ExifTag tag, float d, float m, float s)
            : base(tag, new MathEx.UFraction32[] { new MathEx.UFraction32(d), new MathEx.UFraction32(m), new MathEx.UFraction32(s) })
        {
            ;
        }
    }

    /// <summary>
    /// Represents a GPS time stamp as UTC (EXIF Specification: 3xRATIONAL)
    /// </summary>
    internal class GPSTimeStamp : ExifURationalArray
    {
        protected new MathEx.UFraction32[] Value { get { return mValue; } set { mValue = value; } }
        public MathEx.UFraction32 Hour { get { return mValue[0]; } set { mValue[0] = value; } }
        public MathEx.UFraction32 Minute { get { return mValue[1]; } set { mValue[1] = value; } }
        public MathEx.UFraction32 Second { get { return mValue[2]; } set { mValue[2] = value; } }

        public override string ToString()
        {
            return string.Format("{0:F2}:{1:F2}:{2:F2}\"", (float)Hour, (float)Minute, (float)Second);
        }

        public GPSTimeStamp(ExifTag tag, MathEx.UFraction32[] value)
            : base(tag, value)
        {
            ;
        }

        public GPSTimeStamp(ExifTag tag, float h, float m, float s)
            : base(tag, new MathEx.UFraction32[] { new MathEx.UFraction32(h), new MathEx.UFraction32(m), new MathEx.UFraction32(s) })
        {
            ;
        }
    }

    /// <summary>
    /// Represents an ASCII string. (EXIF Specification: BYTE) 
    /// Used by Windows XP.
    /// </summary>
    internal class WindowsByteString : ExifProperty
    {
        protected string mValue;
        protected override object _Value { get { return Value; } set { Value = (string)value; } }
        public new string Value { get { return mValue; } set { mValue = value; } }

        static public implicit operator string(WindowsByteString obj) { return obj.mValue; }

        public override string ToString() { return mValue; }

        public WindowsByteString(ExifTag tag, string value)
            : base(tag)
        {
            mValue = value;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                byte[] data = Encoding.Unicode.GetBytes(mValue);
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 1, (uint)data.Length, data);
            }
        }
    }
}
