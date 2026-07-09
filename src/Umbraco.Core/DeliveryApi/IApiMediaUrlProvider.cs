using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a provider that retrieves URLs for media items in the Delivery API.
/// </summary>
public interface IApiMediaUrlProvider
{
    /// <summary>
    ///     Gets the URL for the specified media item.
    /// </summary>
    /// <param name="media">The published media to get the URL for.</param>
    /// <returns>The URL of the media item.</returns>
    string GetUrl(IPublishedContent media);
}
