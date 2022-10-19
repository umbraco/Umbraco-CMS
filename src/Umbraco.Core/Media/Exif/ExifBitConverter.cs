using System.Globalization;
using System.Text;

namespace Umbraco.Cms.Core.Media.Exif;

/// <summary>
///     Converts between exif data types and array of bytes.
/// </summary>
internal class ExifBitConverter : BitConverterEx
{
    #region Constructors

    public ExifBitConverter(ByteOrder from, ByteOrder to)
        : base(from, to)
    {
    }

    #endregion

    #region Static Methods

    /// <summary>
    ///     Returns an ASCII string converted from the given byte array.
    /// </summary>
    public static string ToAscii(byte[] data, bool endatfirstnull, Encoding encoding)
    {
        var len = data.Length;
        if (endatfirstnull)
        {
            len = Array.IndexOf(data, (byte)0);
            if (len == -1)
            {
                len = data.Length;
            }
        }

        return encoding.GetString(data, 0, len);
    }

    /// <summary>
    ///     Returns an ASCII string converted from the given byte array.
    /// </summary>
    public static string ToAscii(byte[] data, Encoding encoding) => ToAscii(data, true, encoding);

    /// <summary>
    ///     Returns a string converted from the given byte array.
    ///     from the numeric value of each byte.
    /// </summary>
    public static string ToString(byte[] data)
    {
        var sb = new StringBuilder();
        foreach (var b in data)
        {
            sb.Append(b);
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Returns a DateTime object converted from the given byte array.
    /// </summary>
    public static DateTime ToDateTime(byte[] data, bool hastime)
    {
        var str = ToAscii(data, Encoding.ASCII);
        var parts = str.Split(':', ' ');
        try
        {
            if (hastime && parts.Length == 6)
            {
                // yyyy:MM:dd HH:mm:ss
                // This is the expected format though some cameras
                // can use single digits. See Issue 21.
                return new DateTime(
                    int.Parse(parts[0], CultureInfo.InvariantCulture),
                    int.Parse(parts[1], CultureInfo.InvariantCulture),
                    int.Parse(parts[2], CultureInfo.InvariantCulture),
                    int.Parse(parts[3], CultureInfo.InvariantCulture),
                    int.Parse(parts[4], CultureInfo.InvariantCulture),
                    int.Parse(parts[5], CultureInfo.InvariantCulture));
            }

            if (!hastime && parts.Length == 3)
            {
                // yyyy:MM:dd
                return new DateTime(
                    int.Parse(parts[0], CultureInfo.InvariantCulture),
                    int.Parse(parts[1], CultureInfo.InvariantCulture),
                    int.Parse(parts[2], CultureInfo.InvariantCulture));
            }

            return DateTime.MinValue;
        }
        catch (ArgumentOutOfRangeException)
        {
            return DateTime.MinValue;
        }
        catch (ArgumentException)
        {
            return DateTime.MinValue;
        }
    }

    /// <summary>
    ///     Returns a DateTime object converted from the given byte array.
    /// </summary>
    public static DateTime ToDateTime(byte[] data) => ToDateTime(data, true);

    /// <summary>
    ///     Returns an unsigned rational number converted from the first
    ///     eight bytes of the given byte array. The first four bytes are
    ///     assumed to be the numerator and the next four bytes are the
    ///     denominator.
    ///     Numbers are converted from the given byte-order to platform byte-order.
    /// </summary>
    public static MathEx.UFraction32 ToURational(byte[] data, ByteOrder frombyteorder)
    {
        var num = new byte[4];
        var den = new byte[4];
        Array.Copy(data, 0, num, 0, 4);
        Array.Copy(data, 4, den, 0, 4);
        return new MathEx.UFraction32(
            ToUInt32(num, 0, frombyteorder, SystemByteOrder),
            ToUInt32(den, 0, frombyteorder, SystemByteOrder));
    }

    /// <summary>
    ///     Returns a signed rational number converted from the first
    ///     eight bytes of the given byte array. The first four bytes are
    ///     assumed to be the numerator and the next four bytes are the
    ///     denominator.
    ///     Numbers are converted from the given byte-order to platform byte-order.
    /// </summary>
    public static MathEx.Fraction32 ToSRational(byte[] data, ByteOrder frombyteorder)
    {
        var num = new byte[4];
        var den = new byte[4];
        Array.Copy(data, 0, num, 0, 4);
        Array.Copy(data, 4, den, 0, 4);
        return new MathEx.Fraction32(
            ToInt32(num, 0, frombyteorder, SystemByteOrder),
            ToInt32(den, 0, frombyteorder, SystemByteOrder));
    }

    /// <summary>
    ///     Returns an array of 16-bit unsigned integers converted from
    ///     the given byte array.
    ///     Numbers are converted from the given byte-order to platform byte-order.
    /// </summary>
    public static ushort[] ToUShortArray(byte[] data, int count, ByteOrder frombyteorder)
    {
        var numbers = new ushort[count];
        for (uint i = 0; i < count; i++)
        {
            var num = new byte[2];
            Array.Copy(data, i * 2, num, 0, 2);
            numbers[i] = ToUInt16(num, 0, frombyteorder, SystemByteOrder);
        }

        return numbers;
    }

    /// <summary>
    ///     Returns an array of 32-bit unsigned integers converted from
    ///     the given byte array.
    ///     Numbers are converted from the given byte-order to platform byte-order.
    /// </summary>
    public static uint[] ToUIntArray(byte[] data, int count, ByteOrder frombyteorder)
    {
        var numbers = new uint[count];
        for (uint i = 0; i < count; i++)
        {
            var num = new byte[4];
            Array.Copy(data, i * 4, num, 0, 4);
            numbers[i] = ToUInt32(num, 0, frombyteorder, SystemByteOrder);
        }

        return numbers;
    }

    /// <summary>
    ///     Returns an array of 32-bit signed integers converted from
    ///     the given byte array.
    ///     Numbers are converted from the given byte-order to platform byte-order.
    /// </summary>
    public static int[] ToSIntArray(byte[] data, int count, ByteOrder byteorder)
    {
        var numbers = new int[count];
        for (uint i = 0; i < count; i++)
        {
            var num = new byte[4];
            Array.Copy(data, i * 4, num, 0, 4);
            numbers[i] = ToInt32(num, 0, byteorder, SystemByteOrder);
        }

        return numbers;
    }

    /// <summary>
    ///     Returns an array of unsigned rational numbers converted from
    ///     the given byte array.
    ///     Numbers are converted from the given byte-order to platform byte-order.
    /// </summary>
    public static MathEx.UFraction32[] ToURationalArray(byte[] data, int count, ByteOrder frombyteorder)
    {
        var numbers = new MathEx.UFraction32[count];
        for (uint i = 0; i < count; i++)
        {
            var num = new byte[4];
            var den = new byte[4];
            Array.Copy(data, i * 8, num, 0, 4);
            Array.Copy(data, (i * 8) + 4, den, 0, 4);
            numbers[i].Set(
                ToUInt32(num, 0, frombyteorder, SystemByteOrder),
                ToUInt32(den, 0, frombyteorder, SystemByteOrder));
        }

        return numbers;
    }

    /// <summary>
    ///     Returns an array of signed rational numbers converted from
    ///     the given byte array.
    ///     Numbers are converted from the given byte-order to platform byte-order.
    /// </summary>
    public static MathEx.Fraction32[] ToSRationalArray(byte[] data, int count, ByteOrder frombyteorder)
    {
        var numbers = new MathEx.Fraction32[count];
        for (uint i = 0; i < count; i++)
        {
            var num = new byte[4];
            var den = new byte[4];
            Array.Copy(data, i * 8, num, 0, 4);
            Array.Copy(data, (i * 8) + 4, den, 0, 4);
            numbers[i].Set(
                ToInt32(num, 0, frombyteorder, SystemByteOrder),
                ToInt32(den, 0, frombyteorder, SystemByteOrder));
        }

        return numbers;
    }

    /// <summary>
    ///     Converts the given ascii string to an array of bytes optionally adding a null terminator.
    /// </summary>
    public static byte[] GetBytes(string value, bool addnull, Encoding encoding)
    {
        if (addnull)
        {
            value += '\0';
        }

        return encoding.GetBytes(value);
    }

    /// <summary>
    ///     Converts the given ascii string to an array of bytes without adding a null terminator.
    /// </summary>
    public static byte[] GetBytes(string value, Encoding encoding) => GetBytes(value, false, encoding);

    /// <summary>
    ///     Converts the given datetime to an array of bytes with a null terminator.
    /// </summary>
    public static byte[] GetBytes(DateTime value, bool hastime)
    {
        var str = string.Empty;
        if (hastime)
        {
            str = value.ToString("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);
        }
        else
        {
            str = value.ToString("yyyy:MM:dd", CultureInfo.InvariantCulture);
        }

        return GetBytes(str, true, Encoding.ASCII);
    }

    /// <summary>
    ///     Converts the given unsigned rational number to an array of bytes.
    ///     Numbers are converted from the platform byte-order to the given byte-order.
    /// </summary>
    public static byte[] GetBytes(MathEx.UFraction32 value, ByteOrder tobyteorder)
    {
        var num = GetBytes(value.Numerator, SystemByteOrder, tobyteorder);
        var den = GetBytes(value.Denominator, SystemByteOrder, tobyteorder);
        var data = new byte[8];
        Array.Copy(num, 0, data, 0, 4);
        Array.Copy(den, 0, data, 4, 4);
        return data;
    }

    /// <summary>
    ///     Converts the given signed rational number to an array of bytes.
    ///     Numbers are converted from the platform byte-order to the given byte-order.
    /// </summary>
    public static byte[] GetBytes(MathEx.Fraction32 value, ByteOrder tobyteorder)
    {
        var num = GetBytes(value.Numerator, SystemByteOrder, tobyteorder);
        var den = GetBytes(value.Denominator, SystemByteOrder, tobyteorder);
        var data = new byte[8];
        Array.Copy(num, 0, data, 0, 4);
        Array.Copy(den, 0, data, 4, 4);
        return data;
    }

    /// <summary>
    ///     Converts the given array of 16-bit unsigned integers to an array of bytes.
    ///     Numbers are converted from the platform byte-order to the given byte-order.
    /// </summary>
    public static byte[] GetBytes(ushort[] value, ByteOrder tobyteorder)
    {
        var data = new byte[2 * value.Length];
        for (var i = 0; i < value.Length; i++)
        {
            var num = GetBytes(value[i], SystemByteOrder, tobyteorder);
            Array.Copy(num, 0, data, i * 2, 2);
        }

        return data;
    }

    /// <summary>
    ///     Converts the given array of 32-bit unsigned integers to an array of bytes.
    ///     Numbers are converted from the platform byte-order to the given byte-order.
    /// </summary>
    public static byte[] GetBytes(uint[] value, ByteOrder tobyteorder)
    {
        var data = new byte[4 * value.Length];
        for (var i = 0; i < value.Length; i++)
        {
            var num = GetBytes(value[i], SystemByteOrder, tobyteorder);
            Array.Copy(num, 0, data, i * 4, 4);
        }

        return data;
    }

    /// <summary>
    ///     Converts the given array of 32-bit signed integers to an array of bytes.
    ///     Numbers are converted from the platform byte-order to the given byte-order.
    /// </summary>
    public static byte[] GetBytes(int[] value, ByteOrder tobyteorder)
    {
        var data = new byte[4 * value.Length];
        for (var i = 0; i < value.Length; i++)
        {
            var num = GetBytes(value[i], SystemByteOrder, tobyteorder);
            Array.Copy(num, 0, data, i * 4, 4);
        }

        return data;
    }

    /// <summary>
    ///     Converts the given array of unsigned rationals to an array of bytes.
    ///     Numbers are converted from the platform byte-order to the given byte-order.
    /// </summary>
    public static byte[] GetBytes(MathEx.UFraction32[] value, ByteOrder tobyteorder)
    {
        var data = new byte[8 * value.Length];
        for (var i = 0; i < value.Length; i++)
        {
            var num = GetBytes(value[i].Numerator, SystemByteOrder, tobyteorder);
            var den = GetBytes(value[i].Denominator, SystemByteOrder, tobyteorder);
            Array.Copy(num, 0, data, i * 8, 4);
            Array.Copy(den, 0, data, (i * 8) + 4, 4);
        }

        return data;
    }

    /// <summary>
    ///     Converts the given array of signed rationals to an array of bytes.
    ///     Numbers are converted from the platform byte-order to the given byte-order.
    /// </summary>
    public static byte[] GetBytes(MathEx.Fraction32[] value, ByteOrder tobyteorder)
    {
        var data = new byte[8 * value.Length];
        for (var i = 0; i < value.Length; i++)
        {
            var num = GetBytes(value[i].Numerator, SystemByteOrder, tobyteorder);
            var den = GetBytes(value[i].Denominator, SystemByteOrder, tobyteorder);
            Array.Copy(num, 0, data, i * 8, 4);
            Array.Copy(den, 0, data, (i * 8) + 4, 4);
        }

        return data;
    }

    #endregion
}
