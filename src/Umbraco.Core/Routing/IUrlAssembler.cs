using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides URL assembly functionality for constructing URLs from path components.
/// </summary>
public interface IUrlAssembler
{
    /// <summary>
    ///     Assembles a URL from the specified path, current URI, and URL mode.
    /// </summary>
    /// <param name="path">The path component of the URL.</param>
    /// <param name="current">The current request URI.</param>
    /// <param name="mode">The URL mode determining how the URL should be formatted.</param>
    /// <returns>The assembled <see cref="Uri"/>.</returns>
    Uri AssembleUrl(string path, Uri current, UrlMode mode);
}
