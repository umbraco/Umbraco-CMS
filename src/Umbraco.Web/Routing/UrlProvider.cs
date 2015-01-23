using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Web.PublishedCache;
using Umbraco.Core;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Provides urls.
    /// </summary>
    public class UrlProvider
    {
        #region Ctor and configuration

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlProvider"/> class with an Umbraco context and a list of url providers.
        /// </summary>
        /// <param name="umbracoContext">The Umbraco context.</param>
        /// <param name="routingSettings"></param>
        /// <param name="urlProviders">The list of url providers.</param>
        internal UrlProvider(UmbracoContext umbracoContext, IWebRoutingSection routingSettings, IEnumerable<IUrlProvider> urlProviders)
        {
            _umbracoContext = umbracoContext;
            _urlProviders = urlProviders;

            var provider = UrlProviderMode.Auto;
            Mode = provider;

            if (Enum<UrlProviderMode>.TryParse(routingSettings.UrlProviderMode, out provider))
            {
                Mode = provider;
            }            
        }

        private readonly UmbracoContext _umbracoContext;
        private readonly IEnumerable<IUrlProvider> _urlProviders;

        /// <summary>
        /// Gets or sets the provider url mode.
        /// </summary>
        public UrlProviderMode Mode { get; set; }

        #endregion

        #region GetUrl

        /// <summary>
        /// Gets the url of a published content.
        /// </summary>
        /// <param name="id">The published content identifier.</param>
        /// <returns>The url for the published content.</returns>
        /// <remarks>
        /// <para>The url is absolute or relative depending on <c>Mode</c> and on the current url.</para>
        /// <para>If the provider is unable to provide a url, it returns "#".</para>
        /// </remarks>
        public string GetUrl(int id)
        {
            return GetUrl(id, _umbracoContext.CleanedUmbracoUrl, Mode);
        }

        /// <summary>
        /// Gets the nice url of a published content.
        /// </summary>
        /// <param name="id">The published content identifier.</param>
        /// <param name="absolute">A value indicating whether the url should be absolute in any case.</param>
        /// <returns>The url for the published content.</returns>
        /// <remarks>
        /// <para>The url is absolute or relative depending on <c>Mode</c> and on <c>current</c>, unless
        /// <c>absolute</c> is true, in which case the url is always absolute.</para>
        /// <para>If the provider is unable to provide a url, it returns "#".</para>
        /// </remarks>
        public string GetUrl(int id, bool absolute)
        {
            var mode = absolute ? UrlProviderMode.Absolute : Mode;
            return GetUrl(id, _umbracoContext.CleanedUmbracoUrl, mode);
        }

        /// <summary>
        /// Gets the nice url of a published content.
        /// </summary>
        /// <param name="id">The published content id.</param>
        /// <param name="current">The current absolute url.</param>
        /// <param name="absolute">A value indicating whether the url should be absolute in any case.</param>
        /// <returns>The url for the published content.</returns>
        /// <remarks>
        /// <para>The url is absolute or relative depending on <c>Mode</c> and on <c>current</c>, unless
        /// <c>absolute</c> is true, in which case the url is always absolute.</para>
        /// <para>If the provider is unable to provide a url, it returns "#".</para>
        /// </remarks>
        public string GetUrl(int id, Uri current, bool absolute)
        {
            var mode = absolute ? UrlProviderMode.Absolute : Mode;
            return GetUrl(id, current, mode);
        }

        /// <summary>
        /// Gets the nice url of a published content.
        /// </summary>
        /// <param name="id">The published content identifier.</param>
        /// <param name="mode">The url mode.</param>
        /// <returns>The url for the published content.</returns>
        /// <remarks>
        /// <para>The url is absolute or relative depending on <c>mode</c> and on the current url.</para>
        /// <para>If the provider is unable to provide a url, it returns "#".</para>
        /// </remarks>
        public string GetUrl(int id, UrlProviderMode mode)
        {
            return GetUrl(id, _umbracoContext.CleanedUmbracoUrl, mode);
        }

        /// <summary>
        /// Gets the nice url of a published content.
        /// </summary>
        /// <param name="id">The published content id.</param>
        /// <param name="current">The current absolute url.</param>
        /// <param name="mode">The url mode.</param>
        /// <returns>The url for the published content.</returns>
        /// <remarks>
        /// <para>The url is absolute or relative depending on <c>mode</c> and on <c>current</c>.</para>
        /// <para>If the provider is unable to provide a url, it returns "#".</para>
        /// </remarks>
        public string GetUrl(int id, Uri current, UrlProviderMode mode)
        {
            var url = _urlProviders.Select(provider => provider.GetUrl(_umbracoContext, id, current, mode))
                .FirstOrDefault(u => u != null);
            return url ?? "#"; // legacy wants this
        }

        #endregion

        #region GetOtherUrls

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
            var urls = _urlProviders.SelectMany(provider => provider.GetOtherUrls(_umbracoContext, id, current) ?? Enumerable.Empty<string>());

            return urls;
        }

        #endregion
    }
}
