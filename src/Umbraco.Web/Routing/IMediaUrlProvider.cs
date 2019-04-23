using System;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides media urls.
    /// </summary>
    public interface IMediaUrlProvider
    {
        /// <summary>
        /// Gets the url of a media item.
        /// </summary>
        /// <param name="umbracoContext">The Umbraco context.</param>
        /// <param name="content">The published content.</param>
        /// <param name="propertyAlias">The property alias to resolve the url from.</param>
        /// <param name="mode">The url mode.</param>
        /// <param name="culture">The variation language.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The url for the media.</returns>
        /// <remarks>
        /// <para>The url is absolute or relative depending on <c>mode</c> and on <c>current</c>.</para>
        /// <para>If the media is multi-lingual, gets the url for the specified culture or,
        /// when no culture is specified, the current culture.</para>
        /// <para>The url provider can ignore the mode and always return an absolute url,
        /// e.g. a cdn url provider will most likely always return an absolute url.</para>
        /// <para>If the provider is unable to provide a url, it returns <c>null</c>.</para>
        /// </remarks>
        UrlInfo GetMediaUrl(UmbracoContext umbracoContext, IPublishedContent content, string propertyAlias, UrlProviderMode mode, string culture, Uri current);
    }
}
