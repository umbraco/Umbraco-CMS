using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a content response in the Delivery API that includes culture-specific routes.
/// </summary>
public class ApiContentResponse : ApiContent, IApiContentResponse
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiContentResponse" /> class.
    /// </summary>
    /// <param name="id">The unique identifier of the content.</param>
    /// <param name="name">The name of the content.</param>
    /// <param name="contentType">The content type alias.</param>
    /// <param name="createDate">The date and time when the content was created.</param>
    /// <param name="updateDate">The date and time when the content was last updated.</param>
    /// <param name="route">The route information for the content.</param>
    /// <param name="properties">The property values of the content.</param>
    /// <param name="cultures">The culture-specific routes for the content.</param>
    public ApiContentResponse(Guid id, string name, string contentType, DateTime createDate, DateTime updateDate, IApiContentRoute route, IDictionary<string, object?> properties, IDictionary<string, IApiContentRoute> cultures)
        : base(id, name, contentType, createDate, updateDate, route, properties)
        => Cultures = cultures;

    /// <summary>
    ///     Gets the culture-specific routes for the content, keyed by culture code.
    /// </summary>
    /// <remarks>
    ///     This property is serialized last for better readability of API responses.
    /// </remarks>
    // a little DX; by default this dictionary will be serialized as the first part of the response due to the inner workings of the serializer.
    // that's rather confusing to see as the very first thing when you get a response from the API, so let's move it downwards.
    // hopefully some day System.Text.Json will be able to order the properties differently in a centralized way. for now we have to live
    // with this annotation :(
    [JsonPropertyOrder(100)]
    public IDictionary<string, IApiContentRoute> Cultures { get; }
}
