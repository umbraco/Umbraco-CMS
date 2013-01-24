using System;
using System.Collections.Generic;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides urls.
    /// </summary>
    public interface IUrlProvider
    {
        /// <summary>
        /// Gets the nice url of a published content.
		/// </summary>
        /// <param name="umbracoContext">The Umbraco context.</param>
        /// <param name="id">The published content id.</param>
		/// <param name="current">The current absolute url.</param>
		/// <param name="mode">The url mode.</param>
		/// <returns>The url for the published content.</returns>
		/// <remarks>
        /// <para>The url is absolute or relative depending on <c>mode</c> and on <c>current</c>.</para>
        /// <para>If the provider is unable to provide a url, it should return <c>null</c>.</para>
        /// </remarks>
        string GetUrl(UmbracoContext umbracoContext, int id, Uri current, UrlProviderMode mode);

        /// <summary>
        /// Gets the other urls of a published content.
        /// </summary>
        /// <param name="umbracoContext">The Umbraco context.</param>
        /// <param name="id">The published content id.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The other urls for the published content.</returns>
        /// <remarks>
        /// <para>Other urls are those that <c>GetUrl</c> would not return in the current context, but would be valid
        /// urls for the node in other contexts (different domain for current request, umbracoUrlAlias...).</para>
        /// </remarks>
        IEnumerable<string> GetOtherUrls(UmbracoContext umbracoContext, int id, Uri current);
    }
}
