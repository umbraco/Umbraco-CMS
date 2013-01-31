using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides urls.
    /// </summary>
    internal class UrlProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlProvider"/> class with an Umbraco context, a content cache, and a list of url providers.
        /// </summary>
        /// <param name="umbracoContext">The Umbraco context.</param>
        /// <param name="contentCache">The content cache.</param>
        /// <param name="urlProviders">The list of url providers.</param>
        public UrlProvider(UmbracoContext umbracoContext, IPublishedContentStore contentCache, 
            IEnumerable<IUrlProvider> urlProviders)
        {
            _umbracoContext = umbracoContext;
            _contentCache = contentCache;
            _urlProviders = urlProviders;
            EnforceAbsoluteUrls = false;
        }

        private readonly UmbracoContext _umbracoContext;
        private readonly IPublishedContentStore _contentCache;
        private readonly IEnumerable<IUrlProvider> _urlProviders;

        /// <summary>
        /// Gets or sets a value indicating whether the provider should enforce absolute urls.
        /// </summary>
        public bool EnforceAbsoluteUrls { get; set; }

        /// <summary>
        /// Gets the url of a published content.
        /// </summary>
        /// <param name="id">The published content identifier.</param>
        /// <returns>The url for the published content.</returns>
        /// <remarks>
        /// <para>The url is absolute or relative depending on the current url, settings, and options.</para>
        /// <para>If the provider is unable to provide a url, it returns "#".</para>
        /// </remarks>
        public string GetUrl(int id)
        {
            var absolute = UmbracoSettings.UseDomainPrefixes | EnforceAbsoluteUrls;
            return GetUrl(id, _umbracoContext.CleanedUmbracoUrl, absolute);
        }

        /// <summary>
        /// Gets the nice url of a published content.
        /// </summary>
        /// <param name="id">The published content identifier.</param>
        /// <param name="absolute">A value indicating whether the url should be absolute in any case.</param>
        /// <returns>The url for the published content.</returns>
        /// <remarks>
        /// <para>The url is absolute or relative depending on the current url and settings, unless <c>absolute</c> is true,
        /// in which case the url is always absolute.</para>
        /// <para>If the provider is unable to provide a url, it returns "#".</para>
        /// </remarks>
        public string GetUrl(int id, bool absolute)
        {
            absolute = absolute | EnforceAbsoluteUrls;
            return GetUrl(id, _umbracoContext.CleanedUmbracoUrl, absolute);
        }

        /// <summary>
        /// Gets the nice url of a published content.
        /// </summary>
        /// <param name="id">The published content id.</param>
        /// <param name="current">The current absolute url.</param>
        /// <param name="absolute">A value indicating whether the url should be absolute in any case.</param>
        /// <returns>The url for the published content.</returns>
        /// <remarks>
        /// <para>The url is absolute or relative depending on url indicated by <c>current</c> and settings, unless
        /// <c>absolute</c> is true, in which case the url is always absolute.</para>
        /// <para>If the provider is unable to provide a url, it returns "#".</para>
        /// </remarks>
        public string GetUrl(int id, Uri current, bool absolute)
        {
            absolute = absolute | EnforceAbsoluteUrls;
            var url = _urlProviders.Select(provider => provider.GetUrl(_umbracoContext, _contentCache, id, current, absolute)).FirstOrDefault(u => u != null);
            return url ?? "#"; // legacy wants this
        }

        /// <summary>
        /// Gets the other urls of a published content.
        /// </summary>
        /// <param name="id">The published content id.</param>
        /// <returns>The other urls for the published content.</returns>
        /// <remarks>
        /// <para>Other urls are those that <c>GetUrl</c> would not return in the current context, but would be valid
        /// urls for the node in other contexts (different domain for current request, umbracoUrlAlias...).</para>
        /// <para>The results depend on the current url.</para>
        /// </remarks>
        public IEnumerable<string> GetOtherUrls(int id)
        {
            return GetOtherUrls(id, _umbracoContext.CleanedUmbracoUrl);
        }

        /// <summary>
        /// Gets the other urls of a published content.
        /// </summary>
        /// <param name="id">The published content id.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The other urls for the published content.</returns>
        /// <remarks>
        /// <para>Other urls are those that <c>GetUrl</c> would not return in the current context, but would be valid
        /// urls for the node in other contexts (different domain for current request, umbracoUrlAlias...).</para>
        /// </remarks>
        public IEnumerable<string> GetOtherUrls(int id, Uri current)
        {
            // providers can return null or an empty list or a non-empty list, be prepared
            var urls = _urlProviders.SelectMany(provider => provider.GetOtherUrls(_umbracoContext, _contentCache, id, current) ?? Enumerable.Empty<string>());

            return urls;
        }
    }
}
