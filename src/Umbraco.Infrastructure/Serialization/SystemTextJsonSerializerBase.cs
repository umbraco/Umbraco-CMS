using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Serialization;

public abstract class SystemTextJsonSerializerBase : IJsonSerializer
{
    protected abstract JsonSerializerOptions JsonSerializerOptions { get; }

    /// <inheritdoc />
    public string Serialize(object? input) => JsonSerializer.Serialize(input, JsonSerializerOptions);

    /// <inheritdoc />
    public T? Deserialize<T>(string input) => JsonSerializer.Deserialize<T>(input, JsonSerializerOptions);

    /// <inheritdoc />
    public bool TryDeserialize<T>(object input, [NotNullWhen(true)] out T? value)
        where T : class
    {
        var jsonString = input switch
        {
            JsonNode jsonNodeValue => jsonNodeValue.ToJsonString(),
            string stringValue when stringValue.DetectIsJson() => stringValue,
            _ => null
        };

        value = jsonString.IsNullOrWhiteSpace()
            ? null
            : Deserialize<T>(jsonString);
        return value != null;
    }
}
