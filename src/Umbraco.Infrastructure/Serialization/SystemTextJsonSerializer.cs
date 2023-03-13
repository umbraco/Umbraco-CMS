using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Infrastructure.Serialization;

public class SystemTextJsonSerializer : IJsonSerializer
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public SystemTextJsonSerializer()
    {
        _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        _jsonSerializerOptions.Converters.Add(new JsonUdiConverter());
        _jsonSerializerOptions.Converters.Add(new JsonGuidUdiConverter());
        // we may need to add JsonObjectConverter at some point, but for the time being things work fine without
        // _jsonSerializerOptions.Converters.Add(new JsonObjectConverter());
    }

    public string Serialize(object? input) => JsonSerializer.Serialize(input, _jsonSerializerOptions);

    public T? Deserialize<T>(string input) => JsonSerializer.Deserialize<T>(input, _jsonSerializerOptions);

    public T? DeserializeSubset<T>(string input, string key) => throw new NotSupportedException();
}
