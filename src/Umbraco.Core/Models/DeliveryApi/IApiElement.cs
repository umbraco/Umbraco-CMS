using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a generic element in the Delivery API.
/// </summary>
public interface IApiElement
{
    /// <summary>
    ///     Gets the unique identifier of the element.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    ///     Gets the content type alias of the element.
    /// </summary>
    /// <remarks>
    ///     This property is serialized first to enable use as a discriminator field by System.Text.Json.
    /// </remarks>
    // Ensure the ContentType property is serialized first
    // This is needed so it can be used as a discriminator field by System.Text.Json
    [JsonPropertyOrder(-100)]
    string ContentType { get; }

    /// <summary>
    ///     Gets the property values of the element.
    /// </summary>
    IDictionary<string, object?> Properties { get; }
}
