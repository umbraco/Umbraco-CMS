using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a content response in the Delivery API that includes culture-specific routes.
/// </summary>
public interface IApiContentResponse : IApiContent
{
    /// <summary>
    ///     Gets the culture-specific routes for the content, keyed by culture code.
    /// </summary>
    [JsonPropertyOrder(100)]
    IDictionary<string, IApiContentRoute> Cultures { get; }
}
