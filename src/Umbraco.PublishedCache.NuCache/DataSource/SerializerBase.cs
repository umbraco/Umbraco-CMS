using CSharpTest.Net.Serialization;

namespace Umbraco.Cms.Infrastructure.PublishedCache.DataSource;

internal abstract class SerializerBase
{
    private const char PrefixNull = 'N';
    private const char PrefixString = 'S';
    private const char PrefixInt32 = 'I';
    private const char PrefixUInt16 = 'H';
    private const char PrefixUInt32 = 'J';
    private const char PrefixLong = 'L';
    private const char PrefixFloat = 'F';
    private const char PrefixDouble = 'B';
    private const char PrefixDateTime = 'D';
    private const char PrefixByte = 'O';
    private const char PrefixByteArray = 'A';
    private const char PrefixCompressedStringByteArray = 'C';
    private const char PrefixSignedByte = 'E';
    private const char PrefixBool = 'M';
    private const char PrefixGuid = 'G';
    private const char PrefixTimeSpan = 'T';
    private const char PrefixInt16 = 'Q';
    private const char PrefixChar = 'R';

    protected string ReadString(Stream stream) => PrimitiveSerializer.String.ReadFrom(stream);

    protected int ReadInt(Stream stream) => PrimitiveSerializer.Int32.ReadFrom(stream);

    protected long ReadLong(Stream stream) => PrimitiveSerializer.Int64.ReadFrom(stream);

    protected float ReadFloat(Stream stream) => PrimitiveSerializer.Float.ReadFrom(stream);

    protected double ReadDouble(Stream stream) => PrimitiveSerializer.Double.ReadFrom(stream);

    protected DateTime ReadDateTime(Stream stream) => PrimitiveSerializer.DateTime.ReadFrom(stream);

    protected byte[] ReadByteArray(Stream stream) => PrimitiveSerializer.Bytes.ReadFrom(stream);

    protected string? ReadStringObject(Stream stream, bool intern = false) // required 'cos string is not a struct
    {
        var type = PrimitiveSerializer.Char.ReadFrom(stream);
        if (type == PrefixNull)
        {
            return null;
        }

        if (type != PrefixString)
        {
            throw new NotSupportedException($"Cannot deserialize type '{type}', expected '{PrefixString}'.");
        }

        return intern
            ? string.Intern(PrimitiveSerializer.String.ReadFrom(stream))
            : PrimitiveSerializer.String.ReadFrom(stream);
    }

    private T? ReadStruct<T>(Stream stream, char t, Func<Stream, T> read)
        where T : struct
    {
        var type = PrimitiveSerializer.Char.ReadFrom(stream);
        if (type == PrefixNull)
        {
            return null;
        }

        if (type != t)
        {
            throw new NotSupportedException($"Cannot deserialize type '{type}', expected '{t}'.");
        }

        return read(stream);
    }

    protected int? ReadIntObject(Stream stream) => ReadStruct(stream, PrefixInt32, ReadInt);

    protected long? ReadLongObject(Stream stream) => ReadStruct(stream, PrefixLong, ReadLong);

    protected float? ReadFloatObject(Stream stream) => ReadStruct(stream, PrefixFloat, ReadFloat);

    protected double? ReadDoubleObject(Stream stream) => ReadStruct(stream, PrefixDouble, ReadDouble);

    protected DateTime? ReadDateTimeObject(Stream stream) => ReadStruct(stream, PrefixDateTime, ReadDateTime);

    protected object? ReadObject(Stream stream)
        => ReadObject(PrimitiveSerializer.Char.ReadFrom(stream), stream);

    /// <summary>
    ///     Reads in a value based on its char type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    /// <remarks>
    ///     This will incur boxing because the result is an object but in most cases the value will be a struct.
    ///     When the type is known use the specific methods like <see cref="ReadInt(Stream)" /> instead
    /// </remarks>
    protected object? ReadObject(char type, Stream stream)
    {
        switch (type)
        {
            case PrefixNull:
                return null;
            case PrefixString:
                return PrimitiveSerializer.String.ReadFrom(stream);
            case PrefixInt32:
                return PrimitiveSerializer.Int32.ReadFrom(stream);
            case PrefixUInt16:
                return PrimitiveSerializer.UInt16.ReadFrom(stream);
            case PrefixUInt32:
                return PrimitiveSerializer.UInt32.ReadFrom(stream);
            case PrefixByte:
                return PrimitiveSerializer.Byte.ReadFrom(stream);
            case PrefixLong:
                return PrimitiveSerializer.Int64.ReadFrom(stream);
            case PrefixFloat:
                return PrimitiveSerializer.Float.ReadFrom(stream);
            case PrefixDouble:
                return PrimitiveSerializer.Double.ReadFrom(stream);
            case PrefixDateTime:
                return PrimitiveSerializer.DateTime.ReadFrom(stream);
            case PrefixByteArray:
                return PrimitiveSerializer.Bytes.ReadFrom(stream);
            case PrefixSignedByte:
                return PrimitiveSerializer.SByte.ReadFrom(stream);
            case PrefixBool:
                return PrimitiveSerializer.Boolean.ReadFrom(stream);
            case PrefixGuid:
                return PrimitiveSerializer.Guid.ReadFrom(stream);
            case PrefixTimeSpan:
                return PrimitiveSerializer.TimeSpan.ReadFrom(stream);
            case PrefixInt16:
                return PrimitiveSerializer.Int16.ReadFrom(stream);
            case PrefixChar:
                return PrimitiveSerializer.Char.ReadFrom(stream);
            case PrefixCompressedStringByteArray:
                return new LazyCompressedString(PrimitiveSerializer.Bytes.ReadFrom(stream));
            default:
                throw new NotSupportedException($"Cannot deserialize unknown type '{type}'.");
        }
    }

    /// <summary>
    ///     Writes a value to the stream ensuring it's char type is prefixed to the value for reading later
    /// </summary>
    /// <param name="value"></param>
    /// <param name="stream"></param>
    /// <remarks>
    ///     This method will incur boxing if the value is a struct. When the type is known use the
    ///     <see cref="PrimitiveSerializer" />
    ///     to write the value directly.
    /// </remarks>
    protected void WriteObject(object? value, Stream stream)
    {
        if (value == null)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixNull, stream);
        }
        else if (value is string stringValue)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixString, stream);
            PrimitiveSerializer.String.WriteTo(stringValue, stream);
        }
        else if (value is int intValue)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixInt32, stream);
            PrimitiveSerializer.Int32.WriteTo(intValue, stream);
        }
        else if (value is byte byteValue)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixByte, stream);
            PrimitiveSerializer.Byte.WriteTo(byteValue, stream);
        }
        else if (value is ushort ushortValue)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixUInt16, stream);
            PrimitiveSerializer.UInt16.WriteTo(ushortValue, stream);
        }
        else if (value is long longValue)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixLong, stream);
            PrimitiveSerializer.Int64.WriteTo(longValue, stream);
        }
        else if (value is float floatValue)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixFloat, stream);
            PrimitiveSerializer.Float.WriteTo(floatValue, stream);
        }
        else if (value is double doubleValue)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixDouble, stream);
            PrimitiveSerializer.Double.WriteTo(doubleValue, stream);
        }
        else if (value is DateTime dateValue)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixDateTime, stream);
            PrimitiveSerializer.DateTime.WriteTo(dateValue, stream);
        }
        else if (value is uint uInt32Value)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixUInt32, stream);
            PrimitiveSerializer.UInt32.WriteTo(uInt32Value, stream);
        }
        else if (value is byte[] byteArrayValue)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixByteArray, stream);
            PrimitiveSerializer.Bytes.WriteTo(byteArrayValue, stream);
        }
        else if (value is LazyCompressedString lazyCompressedString)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixCompressedStringByteArray, stream);
            PrimitiveSerializer.Bytes.WriteTo(lazyCompressedString.GetBytes(), stream);
        }
        else if (value is sbyte signedByteValue)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixSignedByte, stream);
            PrimitiveSerializer.SByte.WriteTo(signedByteValue, stream);
        }
        else if (value is bool boolValue)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixBool, stream);
            PrimitiveSerializer.Boolean.WriteTo(boolValue, stream);
        }
        else if (value is Guid guidValue)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixGuid, stream);
            PrimitiveSerializer.Guid.WriteTo(guidValue, stream);
        }
        else if (value is TimeSpan timespanValue)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixTimeSpan, stream);
            PrimitiveSerializer.TimeSpan.WriteTo(timespanValue, stream);
        }
        else if (value is short int16Value)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixInt16, stream);
            PrimitiveSerializer.Int16.WriteTo(int16Value, stream);
        }
        else if (value is char charValue)
        {
            PrimitiveSerializer.Char.WriteTo(PrefixChar, stream);
            PrimitiveSerializer.Char.WriteTo(charValue, stream);
        }
        else
        {
            throw new NotSupportedException("Value type " + value.GetType().FullName + " cannot be serialized.");
        }
    }
}
