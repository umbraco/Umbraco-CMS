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
                    return reader.TryGetDecimal(out decimal decimalValue)
                        ? T.CreateTruncating(decimalValue)
                        : T.CreateTruncating(reader.GetDouble());
                default:
                    throw new JsonException($"Unexpected token '{reader.TokenType}' when parsing a number.");
            }
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
            => writer.WriteRawValue(value.ToString(null, CultureInfo.InvariantCulture) ?? "0");
    }
}
