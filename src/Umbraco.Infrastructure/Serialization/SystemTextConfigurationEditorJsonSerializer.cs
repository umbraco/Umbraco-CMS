using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

// FIXME: clean up all config editor serializers when we can migrate fully to System.Text.Json
// - move this implementation to ConfigurationEditorJsonSerializer (delete the old implementation)
// - use this implementation as the registered singleton (delete ContextualConfigurationEditorJsonSerializer)
// - reuse the JsonObjectConverter implementation from management API (delete the local implementation - pending V12 branch update)

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
    }

    public string Serialize(object? input) => JsonSerializer.Serialize(input, _jsonSerializerOptions);

    public T? Deserialize<T>(string input) => JsonSerializer.Deserialize<T>(input, _jsonSerializerOptions);

    public T? DeserializeSubset<T>(string input, string key) => throw new NotSupportedException();
}
