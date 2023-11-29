using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.DeliveryApi;

public class ApiElement : IApiElement
{
    public ApiElement(Guid id, string contentType, IDictionary<string, object?> properties)
    {
        Id = id;
        ContentType = contentType;
        Properties = properties;
    }

    public Guid Id { get; }

    // Ensure the ContentType property is serialized first
    // This is needed so it can be used as a discriminator field by System.Text.Json
    [JsonPropertyOrder(-100)]
    public string ContentType { get; }

    public IDictionary<string, object?> Properties { get; }
}
