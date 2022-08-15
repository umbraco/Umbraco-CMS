using Newtonsoft.Json;

namespace Umbraco.Cms.Infrastructure.Serialization;

public class FuzzyBooleanConverter : JsonConverter
{
    public override bool CanWrite => false;

    public override bool CanRead => true;

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) =>
        throw new NotSupportedException();

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        var value = reader.Value;
        if (value is bool)
        {
            return value;
        }

        switch (value?.ToString()?.ToLower().Trim())
        {
            case "true":
            case "yes":
            case "y":
            case "1":
                return true;

            case "false":
            case "no":
            case "n":
            case "0":
                return false;
        }

        return new JsonSerializer().Deserialize(reader, objectType);
    }

    public override bool CanConvert(Type objectType) => objectType == typeof(bool);
}
