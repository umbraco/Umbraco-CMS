using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a provider that retrieves the name of content items for the Delivery API.
/// </summary>
public interface IApiContentNameProvider
{
    /// <summary>
    ///     Gets the name of the specified published content.
    /// </summary>
    /// <param name="content">The published content to get the name for.</param>
    /// <returns>The name of the content.</returns>
    string GetName(IPublishedContent content);
}
