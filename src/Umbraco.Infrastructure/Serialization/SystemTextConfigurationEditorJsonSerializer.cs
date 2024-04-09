using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

public class SystemTextConfigurationEditorJsonSerializer : IConfigurationEditorJsonSerializer
{
    private JsonSerializerOptions _jsonSerializerOptions;

    public SystemTextConfigurationEditorJsonSerializer()
    {
        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // in some cases, configs aren't camel cased in the DB, so we have to resort to case insensitive
            // property name resolving when creating configuration objects (deserializing DB configs)
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };
        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        _jsonSerializerOptions.Converters.Add(new JsonObjectConverter());
        _jsonSerializerOptions.Converters.Add(new JsonUdiConverter());
        _jsonSerializerOptions.Converters.Add(new JsonGuidUdiConverter());
        _jsonSerializerOptions.Converters.Add(new JsonBoolConverter());
    }

    public string Serialize(object? input) => JsonSerializer.Serialize(input, _jsonSerializerOptions);

    public T? Deserialize<T>(string input) => JsonSerializer.Deserialize<T>(input, _jsonSerializerOptions);
}
