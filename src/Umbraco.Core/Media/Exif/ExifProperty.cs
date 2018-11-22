using System;
using System.Text;

namespace Umbraco.Core.Media.Exif
{
    /// <summary>
    /// Represents the abstract base class for an Exif property.
    /// </summary>
    internal abstract class ExifProperty
    {
        protected ExifTag mTag;
        protected IFD mIFD;
        protected string mName;

        /// <summary>
        /// Gets the Exif tag associated with this property.
        /// </summary>
        public ExifTag Tag { get { return mTag; } }
        /// <summary>
        /// Gets the IFD section contaning this property.
        /// </summary>
        public IFD IFD { get { return mIFD; } }
        /// <summary>
        /// Gets or sets the name of this property.
        /// </summary>
        public string Name
        {
            get
            {
                if (mName == null || mName.Length == 0)
                    return ExifTagFactory.GetTagName(mTag);
                else
                    return mName;
            }
            set
            {
                mName = value;
            }
        }
        protected abstract object _Value { get; set; }
        /// <summary>
        /// Gets or sets the value of this property.
        /// </summary>
        public object Value { get { return _Value; } set { _Value = value; } }
        /// <summary>
        /// Gets interoperability data for this property.
        /// </summary>
        public abstract ExifInterOperability Interoperability { get; }

        public ExifProperty(ExifTag tag)
        {
            mTag = tag;
            mIFD = ExifTagFactory.GetTagIFD(tag);
        }
    }

    /// <summary>
    /// Represents an 8-bit unsigned integer. (EXIF Specification: BYTE)
    /// </summary>
    internal class ExifByte : ExifProperty
    {
        protected byte mValue;
        protected override object _Value { get { return Value; } set { Value = Convert.ToByte(value); } }
        public new byte Value { get { return mValue; } set { mValue = value; } }

        static public implicit operator byte(ExifByte obj) { return obj.mValue; }

        public override string ToString() { return mValue.ToString(); }

        public ExifByte(ExifTag tag, byte value)
            : base(tag)
        {
            mValue = value;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 1, 1, new byte[] { mValue });
            }
        }
    }

    /// <summary>
    /// Represents an array of 8-bit unsigned integers. (EXIF Specification: BYTE with count > 1)
    /// </summary>
    internal class ExifByteArray : ExifProperty
    {
        protected byte[] mValue;
        protected override object _Value { get { return Value; } set { Value = (byte[])value; } }
        public new byte[] Value { get { return mValue; } set { mValue = value; } }

        static public implicit operator byte[](ExifByteArray obj) { return obj.mValue; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            foreach (byte b in mValue)
            {
                sb.Append(b);
                sb.Append(' ');
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(']');
            return sb.ToString();
        }

        public ExifByteArray(ExifTag tag, byte[] value)
            : base(tag)
        {
            mValue = value;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 1, (uint)mValue.Length, mValue);
            }
        }
    }

    /// <summary>
    /// Represents an ASCII string. (EXIF Specification: ASCII)
    /// </summary>
    internal class ExifAscii : ExifProperty
    {
        protected string mValue;
        protected override object _Value { get { return Value; } set { Value = (string)value; } }
        public new string Value { get { return mValue; } set { mValue = value; } }
        
        public Encoding Encoding { get; private set; }

        static public implicit operator string(ExifAscii obj) { return obj.mValue; }

        public override string ToString() { return mValue; }

        public ExifAscii(ExifTag tag, string value, Encoding encoding)
            : base(tag)
        {
            mValue = value;
            Encoding = encoding;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 2, (uint)mValue.Length + 1, ExifBitConverter.GetBytes(mValue, true, Encoding));
            }
        }
    }

    /// <summary>
    /// Represents a 16-bit unsigned integer. (EXIF Specification: SHORT)
    /// </summary>
    internal class ExifUShort : ExifProperty
    {
        protected ushort mValue;
        protected override object _Value { get { return Value; } set { Value = Convert.ToUInt16(value); } }
        public new ushort Value { get { return mValue; } set { mValue = value; } }

        static public implicit operator ushort(ExifUShort obj) { return obj.mValue; }

        public override string ToString() { return mValue.ToString(); }

        public ExifUShort(ExifTag tag, ushort value)
            : base(tag)
        {
            mValue = value;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 3, 1, ExifBitConverter.GetBytes(mValue, BitConverterEx.SystemByteOrder, BitConverterEx.SystemByteOrder));
            }
        }
    }

    /// <summary>
    /// Represents an array of 16-bit unsigned integers. 
    /// (EXIF Specification: SHORT with count > 1)
    /// </summary>
    internal class ExifUShortArray : ExifProperty
    {
        protected ushort[] mValue;
        protected override object _Value { get { return Value; } set { Value = (ushort[])value; } }
        public new ushort[] Value { get { return mValue; } set { mValue = value; } }

        static public implicit operator ushort[](ExifUShortArray obj) { return obj.mValue; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            foreach (ushort b in mValue)
            {
                sb.Append(b);
                sb.Append(' ');
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(']');
            return sb.ToString();
        }

        public ExifUShortArray(ExifTag tag, ushort[] value)
            : base(tag)
        {
            mValue = value;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 3, (uint)mValue.Length, ExifBitConverter.GetBytes(mValue, BitConverterEx.SystemByteOrder));
            }
        }
    }

    /// <summary>
    /// Represents a 32-bit unsigned integer. (EXIF Specification: LONG)
    /// </summary>
    internal class ExifUInt : ExifProperty
    {
        protected uint mValue;
        protected override object _Value { get { return Value; } set { Value = Convert.ToUInt32(value); } }
        public new uint Value { get { return mValue; } set { mValue = value; } }

        static public implicit operator uint(ExifUInt obj) { return obj.mValue; }

        public override string ToString() { return mValue.ToString(); }

        public ExifUInt(ExifTag tag, uint value)
            : base(tag)
        {
            mValue = value;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 4, 1, ExifBitConverter.GetBytes(mValue, BitConverterEx.SystemByteOrder, BitConverterEx.SystemByteOrder));
            }
        }
    }

    /// <summary>
    /// Represents an array of 16-bit unsigned integers. 
    /// (EXIF Specification: LONG with count > 1)
    /// </summary>
    internal class ExifUIntArray : ExifProperty
    {
        protected uint[] mValue;
        protected override object _Value { get { return Value; } set { Value = (uint[])value; } }
        public new uint[] Value { get { return mValue; } set { mValue = value; } }

        static public implicit operator uint[](ExifUIntArray obj) { return obj.mValue; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            foreach (uint b in mValue)
            {
                sb.Append(b);
                sb.Append(' ');
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(']');
            return sb.ToString();
        }

        public ExifUIntArray(ExifTag tag, uint[] value)
            : base(tag)
        {
            mValue = value;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 3, (uint)mValue.Length, ExifBitConverter.GetBytes(mValue, BitConverterEx.SystemByteOrder));
            }
        }
    }

    /// <summary>
    /// Represents a rational number defined with a 32-bit unsigned numerator 
    /// and denominator. (EXIF Specification: RATIONAL)
    /// </summary>
    internal class ExifURational : ExifProperty
    {
        protected MathEx.UFraction32 mValue;
        protected override object _Value { get { return Value; } set { Value = (MathEx.UFraction32)value; } }
        public new MathEx.UFraction32 Value { get { return mValue; } set { mValue = value; } }

        public override string ToString() { return mValue.ToString(); }
        public float ToFloat() { return (float)mValue; }

        static public explicit operator float(ExifURational obj) { return (float)obj.mValue; }

        public uint[] ToArray()
        {
            return new uint[] { mValue.Numerator, mValue.Denominator };
        }

        public ExifURational(ExifTag tag, uint numerator, uint denominator)
            : base(tag)
        {
            mValue = new MathEx.UFraction32(numerator, denominator);
        }

        public ExifURational(ExifTag tag, MathEx.UFraction32 value)
            : base(tag)
        {
            mValue = value;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 5, 1, ExifBitConverter.GetBytes(mValue, BitConverterEx.SystemByteOrder));
            }
        }
    }

    /// <summary>
    /// Represents an array of unsigned rational numbers. 
    /// (EXIF Specification: RATIONAL with count > 1)
    /// </summary>
    internal class ExifURationalArray : ExifProperty
    {
        protected MathEx.UFraction32[] mValue;
        protected override object _Value { get { return Value; } set { Value = (MathEx.UFraction32[])value; } }
        public new MathEx.UFraction32[] Value { get { return mValue; } set { mValue = value; } }

        static public explicit operator float[](ExifURationalArray obj)
        {
            float[] result = new float[obj.mValue.Length];
            for (int i = 0; i < obj.mValue.Length; i++)
                result[i] = (float)obj.mValue[i];
            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
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

        public ExifURationalArray(ExifTag tag, MathEx.UFraction32[] value)
            : base(tag)
        {
            mValue = value;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 5, (uint)mValue.Length, ExifBitConverter.GetBytes(mValue, BitConverterEx.SystemByteOrder));
            }
        }
    }

    /// <summary>
    /// Represents a byte array that can take any value. (EXIF Specification: UNDEFINED)
    /// </summary>
    internal class ExifUndefined : ExifProperty
    {
        protected byte[] mValue;
        protected override object _Value { get { return Value; } set { Value = (byte[])value; } }
        public new byte[] Value { get { return mValue; } set { mValue = value; } }

        static public implicit operator byte[](ExifUndefined obj) { return obj.mValue; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            foreach (byte b in mValue)
            {
                sb.Append(b);
                sb.Append(' ');
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(']');
            return sb.ToString();
        }

        public ExifUndefined(ExifTag tag, byte[] value)
            : base(tag)
        {
            mValue = value;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 7, (uint)mValue.Length, mValue);
            }
        }
    }

    /// <summary>
    /// Represents a 32-bit signed integer. (EXIF Specification: SLONG)
    /// </summary>
    internal class ExifSInt : ExifProperty
    {
        protected int mValue;
        protected override object _Value { get { return Value; } set { Value = Convert.ToInt32(value); } }
        public new int Value { get { return mValue; } set { mValue = value; } }

        public override string ToString() { return mValue.ToString(); }

        static public implicit operator int(ExifSInt obj) { return obj.mValue; }

        public ExifSInt(ExifTag tag, int value)
            : base(tag)
        {
            mValue = value;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 9, 1, ExifBitConverter.GetBytes(mValue, BitConverterEx.SystemByteOrder, BitConverterEx.SystemByteOrder));
            }
        }
    }

    /// <summary>
    /// Represents an array of 32-bit signed integers. 
    /// (EXIF Specification: SLONG with count > 1)
    /// </summary>
    internal class ExifSIntArray : ExifProperty
    {
        protected int[] mValue;
        protected override object _Value { get { return Value; } set { Value = (int[])value; } }
        public new int[] Value { get { return mValue; } set { mValue = value; } }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            foreach (int b in mValue)
            {
                sb.Append(b);
                sb.Append(' ');
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(']');
            return sb.ToString();
        }

        static public implicit operator int[](ExifSIntArray obj) { return obj.mValue; }

        public ExifSIntArray(ExifTag tag, int[] value)
            : base(tag)
        {
            mValue = value;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 9, (uint)mValue.Length, ExifBitConverter.GetBytes(mValue, BitConverterEx.SystemByteOrder));
            }
        }
    }

    /// <summary>
    /// Represents a rational number defined with a 32-bit signed numerator 
    /// and denominator. (EXIF Specification: SRATIONAL)
    /// </summary>
    internal class ExifSRational : ExifProperty
    {
        protected MathEx.Fraction32 mValue;
        protected override object _Value { get { return Value; } set { Value = (MathEx.Fraction32)value; } }
        public new MathEx.Fraction32 Value { get { return mValue; } set { mValue = value; } }

        public override string ToString() { return mValue.ToString(); }
        public float ToFloat() { return (float)mValue; }

        static public explicit operator float(ExifSRational obj) { return (float)obj.mValue; }

        public int[] ToArray()
        {
            return new int[] { mValue.Numerator, mValue.Denominator };
        }

        public ExifSRational(ExifTag tag, int numerator, int denominator)
            : base(tag)
        {
            mValue = new MathEx.Fraction32(numerator, denominator);
        }

        public ExifSRational(ExifTag tag, MathEx.Fraction32 value)
            : base(tag)
        {
            mValue = value;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 10, 1, ExifBitConverter.GetBytes(mValue, BitConverterEx.SystemByteOrder));
            }
        }
    }

    /// <summary>
    /// Represents an array of signed rational numbers. 
    /// (EXIF Specification: SRATIONAL with count > 1)
    /// </summary>
    internal class ExifSRationalArray : ExifProperty
    {
        protected MathEx.Fraction32[] mValue;
        protected override object _Value { get { return Value; } set { Value = (MathEx.Fraction32[])value; } }
        public new MathEx.Fraction32[] Value { get { return mValue; } set { mValue = value; } }

        static public explicit operator float[](ExifSRationalArray obj)
        {
            float[] result = new float[obj.mValue.Length];
            for (int i = 0; i < obj.mValue.Length; i++)
                result[i] = (float)obj.mValue[i];
            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
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

        public ExifSRationalArray(ExifTag tag, MathEx.Fraction32[] value)
            : base(tag)
        {
            mValue = value;
        }

        public override ExifInterOperability Interoperability
        {
            get
            {
                return new ExifInterOperability(ExifTagFactory.GetTagID(mTag), 10, (uint)mValue.Length, ExifBitConverter.GetBytes(mValue, BitConverterEx.SystemByteOrder));
            }
        }
    }
}
