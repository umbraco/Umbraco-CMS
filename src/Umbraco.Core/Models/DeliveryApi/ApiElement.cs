using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a generic element in the Delivery API.
/// </summary>
public class ApiElement : IApiElement
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiElement" /> class.
    /// </summary>
    /// <param name="id">The unique identifier of the element.</param>
    /// <param name="contentType">The content type alias of the element.</param>
    /// <param name="properties">The property values of the element.</param>
    public ApiElement(Guid id, string contentType, IDictionary<string, object?> properties)
    {
        Id = id;
        ContentType = contentType;
        Properties = properties;
    }

    /// <inheritdoc />
    public Guid Id { get; }

    /// <summary>
    ///     Gets the content type alias of the element.
    /// </summary>
    /// <remarks>
    ///     This property is serialized first to enable use as a discriminator field by System.Text.Json.
    /// </remarks>
    // Ensure the ContentType property is serialized first
    // This is needed so it can be used as a discriminator field by System.Text.Json
    [JsonPropertyOrder(-100)]
    public string ContentType { get; }

    /// <inheritdoc />
    public IDictionary<string, object?> Properties { get; }
}
