using System.Text.Json;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

public class JsonBoolConverter : JsonConverter<bool>
{
    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
        writer.WriteBooleanValue(value);

    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.String => ParseString(ref reader),
            JsonTokenType.Number => reader.TryGetInt64(out long l) ? Convert.ToBoolean(l) : reader.TryGetDouble(out double d) ? Convert.ToBoolean(d) : false,
            _ => throw new JsonException(),
        };

    private bool ParseString(ref Utf8JsonReader reader)
    {
        var value = reader.GetString();
        if (bool.TryParse(value, out var b))
        {
            return b;
        }

        if (int.TryParse(value, out var i))
        {
            return Convert.ToBoolean(i);
        }

        throw new JsonException();
    }
}
