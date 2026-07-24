using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
/// A tolerant JSON converter factory for the numeric value types used in data type configurations.
/// </summary>
/// <remarks>
/// Legacy Umbraco databases (pre-v14) frequently stored numeric configuration values - for example the
/// <c>minNumber</c>/<c>maxNumber</c> of the pickers, or the <c>minVal</c>/<c>step</c> of the slider - as
/// empty strings. The Newtonsoft-based serialization used at the time coerced these to the default value;
/// <see cref="System.Text.Json"/> throws instead. This converter coerces an empty, whitespace or otherwise
/// unparseable string to the type's default (zero), so configurations can still be loaded - and upgrades
/// from such databases run - without a manual data fix.
/// </remarks>
public sealed class JsonTolerantNumberConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
        => typeToConvert == typeof(int)
           || typeToConvert == typeof(long)
           || typeToConvert == typeof(short)
           || typeToConvert == typeof(double)
           || typeToConvert == typeof(float)
           || typeToConvert == typeof(decimal);

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        => (JsonConverter)Activator.CreateInstance(
            typeof(TolerantNumberConverter<>).MakeGenericType(typeToConvert))!;

    private sealed class TolerantNumberConverter<T> : JsonConverter<T>
        where T : struct, INumber<T>
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return T.Zero;
                case JsonTokenType.String:
                    var value = reader.GetString();
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return T.Zero;
                    }

                    return T.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out T parsed)
                        ? parsed
                        : T.Zero;
                case JsonTokenType.Number:
                    return ReadNumber(ref reader);
                default:
                    throw new JsonException($"Unexpected token '{reader.TokenType}' when parsing a number.");
            }
        }

        // Read a JSON number token using the typed reader for the target type, so a value that cannot be
        // represented (e.g. a fractional number for an integer type, or an out-of-range value) resolves to
        // the default rather than being silently truncated.
        private static T ReadNumber(ref Utf8JsonReader reader)
        {
            if (IsInt())
            {
                return reader.TryGetInt32(out var intValue) ? (T)(object)intValue : T.Zero;
            }

            if (IsLong())
            {
                return reader.TryGetInt64(out var longValue) ? (T)(object)longValue : T.Zero;
            }

            if (IsShort())
            {
                return reader.TryGetInt16(out var shortValue) ? (T)(object)shortValue : T.Zero;
            }

            return ReadFloatingPointNumber(ref reader);
        }

        private static T ReadFloatingPointNumber(ref Utf8JsonReader reader)
        {
            if (IsDouble())
            {
                return reader.TryGetDouble(out var doubleValue) ? (T)(object)doubleValue : T.Zero;
            }

            if (IsFloat())
            {
                return reader.TryGetSingle(out var floatValue) ? (T)(object)floatValue : T.Zero;
            }

            return reader.TryGetDecimal(out var decimalValue) ? (T)(object)decimalValue : T.Zero;
        }

        private static bool IsInt() => typeof(T) == typeof(int);

        private static bool IsLong() => typeof(T) == typeof(long);

        private static bool IsShort() => typeof(T) == typeof(short);

        private static bool IsDouble() => typeof(T) == typeof(double);

        private static bool IsFloat() => typeof(T) == typeof(float);

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            // Use the typed numeric writers rather than WriteRawValue, which can emit invalid JSON for
            // floating-point edge cases.
            if (IsInt())
            {
                writer.WriteNumberValue((int)(object)value);
            }
            else if (IsLong())
            {
                writer.WriteNumberValue((long)(object)value);
            }
            else if (IsShort())
            {
                writer.WriteNumberValue((int)(short)(object)value);
            }
            else if (IsDouble())
            {
                writer.WriteNumberValue((double)(object)value);
            }
            else if (IsFloat())
            {
                writer.WriteNumberValue((float)(object)value);
            }
            else
            {
                writer.WriteNumberValue((decimal)(object)value);
            }
        }
    }
}
