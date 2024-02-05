using System.Text.Json;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

public class AutoInterningStringConverter : JsonConverter<string>
{
    // This is a hacky workaround to creating a "read only converter", since System.Text.Json doesn't support it.
    // Taken from https://github.com/dotnet/runtime/issues/46372#issuecomment-1660515178
    private readonly JsonConverter<string> _fallbackConverter = (JsonConverter<string>)JsonSerializerOptions.Default.GetConverter(typeof(string));

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.String =>
                // It's safe to ignore nullability here, because according to the docs:
                // Returns null when TokenType is JsonTokenType.Null
                // https://learn.microsoft.com/en-us/dotnet/api/system.text.json.utf8jsonreader.getstring?view=net-8.0#remarks
                string.Intern(reader.GetString()!),
            _ => throw new InvalidOperationException($"{nameof(AutoInterningStringConverter)} only supports strings."),
        };

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        => _fallbackConverter.Write(writer, value, options);
}
