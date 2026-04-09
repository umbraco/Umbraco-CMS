using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a resolver that finds published content by its URL path.
/// </summary>
public interface IApiContentPathResolver
{
    /// <summary>
    ///     Resolves published content from the specified URL path.
    /// </summary>
    /// <param name="path">The URL path to resolve.</param>
    /// <returns>The published content at the specified path, or <c>null</c> if not found.</returns>
    IPublishedContent? ResolveContentPath(string path);
}
