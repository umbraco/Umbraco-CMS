using System.Text.Json;

namespace Umbraco.Cms.Api.Management.Serialization;

// TOOD: move this to Infrastructure.Serialization + get rid of ISystemTextJsonSerializer
public class SystemTextJsonSerializer : ISystemTextJsonSerializer
{
    private JsonSerializerOptions _jsonSerializerOptions;
    public SystemTextJsonSerializer()
    {
        _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }
    public string Serialize(object? input) => JsonSerializer.Serialize(input, _jsonSerializerOptions);

    public T? Deserialize<T>(string input) => JsonSerializer.Deserialize<T>(input, _jsonSerializerOptions);

    public T? DeserializeSubset<T>(string input, string key) => throw new NotSupportedException();
}
