using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Serialization;

public class JsonUdiConverter : JsonConverter<Udi>
{
    public override Udi? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var stringValue = reader.GetString();
        return stringValue.IsNullOrWhiteSpace() == false && UdiParser.TryParse(stringValue, out Udi? udi)
            ? udi
            : null;
    }

    public override void Write(Utf8JsonWriter writer, Udi value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
