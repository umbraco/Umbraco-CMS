using System.Text.Json;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
/// Converts a boolean value to or from JSON, always converting a boolean like value (like <c>1</c> or <c>0</c>) to a boolean.
/// </summary>
public sealed class JsonBooleanConverter : JsonConverter<bool>
{
    /// <inheritdoc />
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.String => ParseString(ref reader),
            JsonTokenType.Number => ParseNumber(ref reader),
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            _ => throw new JsonException(),
        };

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        => writer.WriteBooleanValue(value);

    private static bool ParseString(ref Utf8JsonReader reader)
    {
        var value = reader.GetString();

        if (bool.TryParse(value, out var boolValue))
        {
            return boolValue;
        }

        if (long.TryParse(value, out var longValue))
        {
            return Convert.ToBoolean(longValue);
        }

        if (double.TryParse(value, out double doubleValue))
        {
            return Convert.ToBoolean(doubleValue);
        }

        throw new JsonException();
    }

    private static bool ParseNumber(ref Utf8JsonReader reader)
    {
        if (reader.TryGetInt64(out long longValue))
        {
            return Convert.ToBoolean(longValue);
        }

        if (reader.TryGetDouble(out double doubleValue))
        {
            return Convert.ToBoolean(doubleValue);
        }

        throw new JsonException();
    }
}
