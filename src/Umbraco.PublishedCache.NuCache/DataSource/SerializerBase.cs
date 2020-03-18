using System;
using System.IO;
using CSharpTest.Net.Serialization;

namespace Umbraco.Web.PublishedCache.NuCache.DataSource
{
    internal abstract class SerializerBase
    {
        protected string ReadString(Stream stream) => PrimitiveSerializer.String.ReadFrom(stream);
        protected int ReadInt(Stream stream) => PrimitiveSerializer.Int32.ReadFrom(stream);
        protected long ReadLong(Stream stream) => PrimitiveSerializer.Int64.ReadFrom(stream);
        protected float ReadFloat(Stream stream) => PrimitiveSerializer.Float.ReadFrom(stream);
        protected double ReadDouble(Stream stream) => PrimitiveSerializer.Double.ReadFrom(stream);
        protected DateTime ReadDateTime(Stream stream) => PrimitiveSerializer.DateTime.ReadFrom(stream);

        private T? ReadObject<T>(Stream stream, char t, Func<Stream, T> read)
            where T : struct
        {
            var type = PrimitiveSerializer.Char.ReadFrom(stream);
            if (type == 'N') return null;
            if (type != t)
                throw new NotSupportedException($"Cannot deserialize type '{type}', expected '{t}'.");
            return read(stream);
        }

        protected string ReadStringObject(Stream stream) // required 'cos string is not a struct
        {
            var type = PrimitiveSerializer.Char.ReadFrom(stream);
            if (type == 'N') return null;
            if (type != 'S')
                throw new NotSupportedException($"Cannot deserialize type '{type}', expected 'S'.");
            return PrimitiveSerializer.String.ReadFrom(stream);
        }

        protected int? ReadIntObject(Stream stream) => ReadObject(stream, 'I', ReadInt);
        protected long? ReadLongObject(Stream stream) => ReadObject(stream, 'L', ReadLong);
        protected float? ReadFloatObject(Stream stream) => ReadObject(stream, 'F', ReadFloat);
        protected double? ReadDoubleObject(Stream stream) => ReadObject(stream, 'B', ReadDouble);
        protected DateTime? ReadDateTimeObject(Stream stream) => ReadObject(stream, 'D', ReadDateTime);

        protected object ReadObject(Stream stream)
            => ReadObject(PrimitiveSerializer.Char.ReadFrom(stream), stream);

        protected object ReadObject(char type, Stream stream)
        {
            switch (type)
            {
                case 'N':
                    return null;
                case 'S':
                    return PrimitiveSerializer.String.ReadFrom(stream);
                case 'I':
                    return PrimitiveSerializer.Int32.ReadFrom(stream);
                case 'L':
                    return PrimitiveSerializer.Int64.ReadFrom(stream);
                case 'F':
                    return PrimitiveSerializer.Float.ReadFrom(stream);
                case 'B':
                    return PrimitiveSerializer.Double.ReadFrom(stream);
                case 'D':
                    return PrimitiveSerializer.DateTime.ReadFrom(stream);
                default:
                    throw new NotSupportedException($"Cannot deserialize unknown type '{type}'.");
            }
        }

        protected void WriteObject(object value, Stream stream)
        {
            if (value == null)
            {
                PrimitiveSerializer.Char.WriteTo('N', stream);
            }
            else if (value is string stringValue)
            {
                PrimitiveSerializer.Char.WriteTo('S', stream);
                PrimitiveSerializer.String.WriteTo(stringValue, stream);
            }
            else if (value is int intValue)
            {
                PrimitiveSerializer.Char.WriteTo('I', stream);
                PrimitiveSerializer.Int32.WriteTo(intValue, stream);
            }
            else if (value is long longValue)
            {
                PrimitiveSerializer.Char.WriteTo('L', stream);
                PrimitiveSerializer.Int64.WriteTo(longValue, stream);
            }
            else if (value is float floatValue)
            {
                PrimitiveSerializer.Char.WriteTo('F', stream);
                PrimitiveSerializer.Float.WriteTo(floatValue, stream);
            }
            else if (value is double doubleValue)
            {
                PrimitiveSerializer.Char.WriteTo('B', stream);
                PrimitiveSerializer.Double.WriteTo(doubleValue, stream);
            }
            else if (value is DateTime dateValue)
            {
                PrimitiveSerializer.Char.WriteTo('D', stream);
                PrimitiveSerializer.DateTime.WriteTo(dateValue, stream);
            }
            else
                throw new NotSupportedException("Value type " + value.GetType().FullName + " cannot be serialized.");
        }
    }
}
