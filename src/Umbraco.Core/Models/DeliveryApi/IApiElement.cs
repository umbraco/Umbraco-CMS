using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.DeliveryApi;

public interface IApiElement
{
    Guid Id { get; }

    // Ensure the ContentType property is serialized first
    // This is needed so it can be used as a discriminator field by System.Text.Json
    [JsonPropertyOrder(-100)]
    string ContentType { get; }

    IDictionary<string, object?> Properties { get; }
}
