using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

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
        /// <param name="routingSettings">Routing settings.</param>
        /// <param name="urlProviders">The list of url providers.</param>
        /// <param name="variationContextAccessor">The current variation accessor.</param>
        public UrlProvider(UmbracoContext umbracoContext, IWebRoutingSection routingSettings, IEnumerable<IUrlProvider> urlProviders, IVariationContextAccessor variationContextAccessor)
        {
            if (routingSettings == null) throw new ArgumentNullException(nameof(routingSettings));

            _umbracoContext = umbracoContext ?? throw new ArgumentNullException(nameof(umbracoContext));
            _urlProviders = urlProviders;
            _variationContextAccessor = variationContextAccessor ?? throw new ArgumentNullException(nameof(variationContextAccessor));
            var provider = UrlProviderMode.Auto;
            Mode = provider;

            if (Enum<UrlProviderMode>.TryParse(routingSettings.UrlProviderMode, out provider))
            {
                Mode = provider;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlProvider"/> class with an Umbraco context and a list of url providers.
        /// </summary>
        /// <param name="umbracoContext">The Umbraco context.</param>
        /// <param name="urlProviders">The list of url providers.</param>
        /// <param name="variationContextAccessor">The current variation accessor.</param>
        /// <param name="mode">An optional provider mode.</param>
        public UrlProvider(UmbracoContext umbracoContext, IEnumerable<IUrlProvider> urlProviders, IVariationContextAccessor variationContextAccessor, UrlProviderMode mode = UrlProviderMode.Auto)
        {
            _umbracoContext = umbracoContext ?? throw new ArgumentNullException(nameof(umbracoContext));
            _urlProviders = urlProviders;
            _variationContextAccessor = variationContextAccessor;

            Mode = mode;
        }

        private readonly UmbracoContext _umbracoContext;
        private readonly IEnumerable<IUrlProvider> _urlProviders;
        private readonly IVariationContextAccessor _variationContextAccessor;

        /// <summary>
        /// Gets or sets the provider url mode.
        /// </summary>
        public UrlProviderMode Mode { get; set; }

        #endregion

        #region GetUrl

        private UrlProviderMode GetMode(bool absolute) => absolute ? UrlProviderMode.Absolute : Mode;
        private IPublishedContent GetDocument(int id) => _umbracoContext.ContentCache.GetById(id);
        private IPublishedContent GetDocument(Guid id) => _umbracoContext.ContentCache.GetById(id);

        /// <summary>
        /// Gets the url of a published content.
        /// </summary>
        /// <param name="content">The published content.</param>
        /// <param name="culture">A culture.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The url for the published content.</returns>
        public string GetUrl(IPublishedContent content, string culture = null, Uri current = null)
            => GetUrl(content, Mode, culture, current);

        /// <summary>
        /// Gets the nice url of a published content.
        /// </summary>
        /// <param name="content">The published content.</param>
        /// <param name="absolute">A value indicating whether the url should be absolute in any case.</param>
        /// <param name="culture">A culture.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The url for the published content.</returns>
        /// <remarks>
        /// <para>The url is absolute or relative depending on <c>Mode</c> and on <c>current</c>, unless
        /// <c>absolute</c> is true, in which case the url is always absolute.</para>
        /// </remarks>
        public string GetUrl(IPublishedContent content, bool absolute, string culture = null, Uri current = null)
            => GetUrl(content, GetMode(absolute), culture, current);

        /// <summary>
        /// Gets the url of a published content.
        /// </summary>
        /// <param name="id">The published content identifier.</param>
        /// <param name="culture">A culture.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The url for the published content.</returns>
        public string GetUrl(Guid id, string culture = null, Uri current = null)
            => GetUrl(GetDocument(id), Mode, culture, current);

        /// <summary>
        /// Gets the nice url of a published content.
        /// </summary>
        /// <param name="id">The published content identifier.</param>
        /// <param name="absolute">A value indicating whether the url should be absolute in any case.</param>
        /// <param name="culture">A culture.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The url for the published content.</returns>
        /// <remarks>
        /// <para>The url is absolute or relative depending on <c>Mode</c> and on <c>current</c>, unless
        /// <c>absolute</c> is true, in which case the url is always absolute.</para>
        /// </remarks>
        public string GetUrl(Guid id, bool absolute, string culture = null, Uri current = null)
            => GetUrl(GetDocument(id), GetMode(absolute), culture, current);

        /// <summary>
        /// Gets the nice url of a published content.
        /// </summary>
        /// <param name="id">The published content identifier.</param>
        /// <param name="mode">The url mode.</param>
        /// <param name="culture">A culture.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The url for the published content.</returns>
        public string GetUrl(Guid id, UrlProviderMode mode, string culture = null, Uri current = null)
            => GetUrl(GetDocument(id), mode, culture, current);

        /// <summary>
        /// Gets the url of a published content.
        /// </summary>
        /// <param name="id">The published content identifier.</param>
        /// <param name="culture">A culture.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The url for the published content.</returns>
        public string GetUrl(int id, string culture = null, Uri current = null)
            => GetUrl(GetDocument(id), Mode, culture, current);

        /// <summary>
        /// Gets the nice url of a published content.
        /// </summary>
        /// <param name="id">The published content identifier.</param>
        /// <param name="absolute">A value indicating whether the url should be absolute in any case.</param>
        /// <param name="culture">A culture.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The url for the published content.</returns>
        /// <remarks>
        /// <para>The url is absolute or relative depending on <c>Mode</c> and on <c>current</c>, unless
        /// <c>absolute</c> is true, in which case the url is always absolute.</para>
        /// </remarks>
        public string GetUrl(int id, bool absolute, string culture = null, Uri current = null)
            => GetUrl(GetDocument(id), GetMode(absolute), culture, current);

        /// <summary>
        /// Gets the nice url of a published content.
        /// </summary>
        /// <param name="id">The published content identifier.</param>
        /// <param name="mode">The url mode.</param>
        /// <param name="culture">A culture.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The url for the published content.</returns>
        public string GetUrl(int id, UrlProviderMode mode, string culture = null, Uri current = null)
            => GetUrl(GetDocument(id), mode, culture, current);

        /// <summary>
        /// Gets the nice url of a published content.
        /// </summary>
        /// <param name="content">The published content.</param>
        /// <param name="mode">The url mode.</param>
        /// <param name="culture">A culture.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The url for the published content.</returns>
        /// <remarks>
        /// <para>The url is absolute or relative depending on <c>mode</c> and on <c>current</c>.</para>
        /// <para>If the published content is multi-lingual, gets the url for the specified culture or,
        /// when no culture is specified, the current culture.</para>
        /// <para>If the provider is unable to provide a url, it returns "#".</para>
        /// </remarks>
        public string GetUrl(IPublishedContent content, UrlProviderMode mode, string culture = null, Uri current = null)
        {
            if (content == null || content.ItemType == PublishedItemType.Element)
                return "#";

            // this the ONLY place where we deal with default culture - IUrlProvider always receive a culture
            // be nice with tests, assume things can be null, ultimately fall back to invariant
            // (but only for variant content of course)
            if (content.ContentType.VariesByCulture())
            {
                if (culture == null)
                    culture = _variationContextAccessor?.VariationContext?.Culture ?? "";
            }

            if (current == null)
                current = _umbracoContext.CleanedUmbracoUrl;

            var url = _urlProviders.Select(provider => provider.GetUrl(_umbracoContext, content, mode, culture, current))
                .FirstOrDefault(u => u != null);
            return url?.Text ?? "#"; // legacy wants this
        }

        internal string GetUrlFromRoute(int id, string route, string culture)
        {
            var provider = _urlProviders.OfType<DefaultUrlProvider>().FirstOrDefault();
            var url = provider == null
                ? route // what else?
                : provider.GetUrlFromRoute(route, Current.UmbracoContext, id, _umbracoContext.CleanedUmbracoUrl, Mode, culture)?.Text;
            return url ?? "#";
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
        public IEnumerable<UrlInfo> GetOtherUrls(int id)
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
        public IEnumerable<UrlInfo> GetOtherUrls(int id, Uri current)
        {
            return _urlProviders.SelectMany(provider => provider.GetOtherUrls(_umbracoContext, id, current));
        }

        #endregion
    }
}
