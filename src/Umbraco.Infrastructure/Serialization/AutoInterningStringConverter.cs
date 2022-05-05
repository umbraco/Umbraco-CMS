using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Umbraco.Cms.Infrastructure.Serialization;

/// <summary>
///     When applied to a string or string collection field will ensure the deserialized strings are interned
/// </summary>
/// <remarks>
///     Borrowed from https://stackoverflow.com/a/34906004/694494
///     On the same page an interesting approach of using a local intern pool https://stackoverflow.com/a/39605620/694494
///     which re-uses .NET System.Xml.NameTable
/// </remarks>
public class AutoInterningStringConverter : JsonConverter
{
    public override bool CanWrite => false;

    public override bool CanConvert(Type objectType) => throw

        // CanConvert is not called when a converter is applied directly to a property.
        new NotImplementedException($"{nameof(AutoInterningStringConverter)} should not be used globally");

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
        {
            return null;
        }

        // Check is in case the value is a non-string literal such as an integer.
        var s = reader.TokenType == JsonToken.String
            ? string.Intern((string)reader.Value!)
            : string.Intern((string)JToken.Load(reader)!);
        return s;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
        throw new NotImplementedException();
}
