﻿using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Routing
{

    /// <summary>
    /// Provides urls.
    /// </summary>
    public class UrlProvider : IPublishedUrlProvider
    {
        #region Ctor and configuration

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlProvider"/> class with an Umbraco context and a list of url providers.
        /// </summary>
        /// <param name="umbracoContextAccessor">The Umbraco context accessor.</param>
        /// <param name="routingSettings">Routing settings.</param>
        /// <param name="urlProviders">The list of url providers.</param>
        /// <param name="mediaUrlProviders">The list of media url providers.</param>
        /// <param name="variationContextAccessor">The current variation accessor.</param>
        /// <param name="propertyEditorCollection"></param>
        public UrlProvider(IUmbracoContextAccessor umbracoContextAccessor, IWebRoutingSection routingSettings, UrlProviderCollection urlProviders, MediaUrlProviderCollection mediaUrlProviders, IVariationContextAccessor variationContextAccessor)
        {
            if (routingSettings == null) throw new ArgumentNullException(nameof(routingSettings));

            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _urlProviders = urlProviders;
            _mediaUrlProviders = mediaUrlProviders;
            _variationContextAccessor = variationContextAccessor ?? throw new ArgumentNullException(nameof(variationContextAccessor));
            var provider = UrlMode.Auto;
            Mode = provider;

            if (Enum<UrlMode>.TryParse(routingSettings.UrlProviderMode, out provider))
            {
                Mode = provider;
            }
        }


        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IEnumerable<IUrlProvider> _urlProviders;
        private readonly IEnumerable<IMediaUrlProvider> _mediaUrlProviders;
        private readonly IVariationContextAccessor _variationContextAccessor;

        /// <summary>
        /// Gets or sets the provider url mode.
        /// </summary>
        public UrlMode Mode { get; set; }

        #endregion

        #region GetUrl

        private IPublishedContent GetDocument(int id) => _umbracoContextAccessor.UmbracoContext.Content.GetById(id);
        private IPublishedContent GetDocument(Guid id) => _umbracoContextAccessor.UmbracoContext.Content.GetById(id);
        private IPublishedContent GetMedia(Guid id) => _umbracoContextAccessor.UmbracoContext.Media.GetById(id);

        /// <summary>
        /// Gets the url of a published content.
        /// </summary>
        /// <param name="id">The published content identifier.</param>
        /// <param name="mode">The url mode.</param>
        /// <param name="culture">A culture.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The url for the published content.</returns>
        public string GetUrl(Guid id, UrlMode mode = UrlMode.Default, string culture = null, Uri current = null)
            => GetUrl(GetDocument(id), mode, culture, current);

        /// <summary>
        /// Gets the url of a published content.
        /// </summary>
        /// <param name="id">The published content identifier.</param>
        /// <param name="mode">The url mode.</param>
        /// <param name="culture">A culture.</param>
        /// <param name="current">The current absolute url.</param>
        /// <returns>The url for the published content.</returns>
        public string GetUrl(int id, UrlMode mode = UrlMode.Default, string culture = null, Uri current = null)
            => GetUrl(GetDocument(id), mode, culture, current);

        /// <summary>
        /// Gets the url of a published content.
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
        public string GetUrl(IPublishedContent content, UrlMode mode = UrlMode.Default, string culture = null, Uri current = null)
        {
            if (content == null || content.ContentType.ItemType == PublishedItemType.Element)
                return "#";

            if (mode == UrlMode.Default)
                mode = Mode;

            // this the ONLY place where we deal with default culture - IUrlProvider always receive a culture
            // be nice with tests, assume things can be null, ultimately fall back to invariant
            // (but only for variant content of course)
            if (content.ContentType.VariesByCulture())
            {
                if (culture == null)
                    culture = _variationContextAccessor?.VariationContext?.Culture ?? "";
            }

            if (current == null)
                current = _umbracoContextAccessor.UmbracoContext.CleanedUmbracoUrl;

            var url = _urlProviders.Select(provider => provider.GetUrl(content, mode, culture, current))
                .FirstOrDefault(u => u != null);
            return url?.Text ?? "#"; // legacy wants this
        }

        public string GetUrlFromRoute(int id, string route, string culture)
        {
            var provider = _urlProviders.OfType<DefaultUrlProvider>().FirstOrDefault();
            var url = provider == null
                ? route // what else?
                : provider.GetUrlFromRoute(route, _umbracoContextAccessor.UmbracoContext, id, _umbracoContextAccessor.UmbracoContext.CleanedUmbracoUrl, Mode, culture)?.Text;
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
            return GetOtherUrls(id, _umbracoContextAccessor.UmbracoContext.CleanedUmbracoUrl);
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
            return _urlProviders.SelectMany(provider => provider.GetOtherUrls(id, current) ?? Enumerable.Empty<UrlInfo>());
        }

        #endregion

        #region GetMediaUrl

        /// <summary>
        /// Gets the url of a media item.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="mode"></param>
        /// <param name="culture"></param>
        /// <param name="propertyAlias"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        public string GetMediaUrl(Guid id, UrlMode mode = UrlMode.Default, string culture = null, string propertyAlias = Constants.Conventions.Media.File, Uri current = null)
            => GetMediaUrl(GetMedia(id), mode, culture, propertyAlias, current);

        /// <summary>
        /// Gets the url of a media item.
        /// </summary>
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
        /// <para>If the provider is unable to provide a url, it returns <see cref="String.Empty"/>.</para>
        /// </remarks>
        public string GetMediaUrl(IPublishedContent content, UrlMode mode = UrlMode.Default, string culture = null, string propertyAlias = Constants.Conventions.Media.File, Uri current = null)
        {
            if (propertyAlias == null) throw new ArgumentNullException(nameof(propertyAlias));

            if (content == null)
                return "";

            if (mode == UrlMode.Default)
                mode = Mode;

            // this the ONLY place where we deal with default culture - IMediaUrlProvider always receive a culture
            // be nice with tests, assume things can be null, ultimately fall back to invariant
            // (but only for variant content of course)
            if (content.ContentType.VariesByCulture())
            {
                if (culture == null)
                    culture = _variationContextAccessor?.VariationContext?.Culture ?? "";
            }

            if (current == null)
                current = _umbracoContextAccessor.UmbracoContext.CleanedUmbracoUrl;

            var url = _mediaUrlProviders.Select(provider =>
                    provider.GetMediaUrl(content, propertyAlias, mode, culture, current))
                .FirstOrDefault(u => u != null);

            return url?.Text ?? "";
        }

        #endregion
    }
}
