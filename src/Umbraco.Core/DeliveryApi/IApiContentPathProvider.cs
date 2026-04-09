using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a provider that retrieves the URL path of content items for the Delivery API.
/// </summary>
public interface IApiContentPathProvider
{
    /// <summary>
    ///     Gets the URL path for the specified published content and culture.
    /// </summary>
    /// <param name="content">The published content to get the path for.</param>
    /// <param name="culture">The culture to use for the path, or <c>null</c> for the default culture.</param>
    /// <returns>The URL path of the content, or <c>null</c> if no path is available.</returns>
    string? GetContentPath(IPublishedContent content, string? culture);
}
