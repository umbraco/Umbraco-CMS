namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a content response in the Delivery API that includes culture-specific routes.
/// </summary>
public interface IApiContentResponse : IApiContent
{
    /// <summary>
    ///     Gets the culture-specific routes for the content, keyed by culture code.
    /// </summary>
    IDictionary<string, IApiContentRoute> Cultures { get; }
}
