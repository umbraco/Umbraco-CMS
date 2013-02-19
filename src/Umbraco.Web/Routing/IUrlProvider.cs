using System;
using System.Collections.Generic;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides urls.
    /// </summary>
    internal interface IUrlProvider
    {
        /// <summary>
        /// Gets the nice url of a published content.
		/// </summary>
        /// <param name="umbracoContext">The Umbraco context.</param>
        /// <param name="contentCache">The content cache.</param>
        /// <param name="id">The published content id.</param>
		/// <param name="current">The current absolute url.</param>
		/// <param name="absolute">A value indicating whether the url should be absolute in any case.</param>
		/// <returns>The url for the published content.</returns>
		/// <remarks>
        /// <para>The url is absolute or relative depending on url indicated by <c>current</c> and settings, unless
        /// <c>absolute</c> is true, in which case the url is always absolute.</para>
        /// <para>If the provider is unable to provide a url, it should return <c>null</c>.</para>
        /// </remarks>
        string GetUrl(UmbracoContext umbracoContext, IPublishedContentStore contentCache, int id, Uri current, bool absolute);

        /// <summary>
        /// Gets the other urls of a published content.
        /// </summary>
        /// <param name="umbracoContext">The Umbraco context.</param>
        /// <param name="contentCache">The content cache.</param>
        /// <param name="id">The published content id.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The other urls for the published content.</returns>
        /// <remarks>
        /// <para>Other urls are those that <c>GetUrl</c> would not return in the current context, but would be valid
        /// urls for the node in other contexts (different domain for current request, umbracoUrlAlias...).</para>
        /// </remarks>
        IEnumerable<string> GetOtherUrls(UmbracoContext umbracoContext, IPublishedContentStore contentCache, int id, Uri current);
    }
}
