using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Events;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Web
{

    /// <summary>
    /// Class that encapsulates Umbraco information of a specific HTTP request
    /// </summary>
    public class UmbracoContext : DisposableObjectSlim, IDisposeOnRequestEnd
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly Lazy<IPublishedSnapshot> _publishedSnapshot;
        private string _previewToken;
        private bool? _previewing;

        // initializes a new instance of the UmbracoContext class
        // internal for unit tests
        // otherwise it's used by EnsureContext above
        // warn: does *not* manage setting any IUmbracoContextAccessor
        internal UmbracoContext(HttpContextBase httpContext,
            IPublishedSnapshotService publishedSnapshotService,
            WebSecurity webSecurity,
            IUmbracoSettingsSection umbracoSettings,
            IEnumerable<IUrlProvider> urlProviders,
            IEnumerable<IMediaUrlProvider> mediaUrlProviders,
            IGlobalSettings globalSettings,
            IVariationContextAccessor variationContextAccessor)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (publishedSnapshotService == null) throw new ArgumentNullException(nameof(publishedSnapshotService));
            if (webSecurity == null) throw new ArgumentNullException(nameof(webSecurity));
            if (umbracoSettings == null) throw new ArgumentNullException(nameof(umbracoSettings));
            if (urlProviders == null) throw new ArgumentNullException(nameof(urlProviders));
            if (mediaUrlProviders == null) throw new ArgumentNullException(nameof(mediaUrlProviders));
            VariationContextAccessor = variationContextAccessor ??  throw new ArgumentNullException(nameof(variationContextAccessor));
            _globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));

            // ensure that this instance is disposed when the request terminates, though we *also* ensure
            // this happens in the Umbraco module since the UmbracoCOntext is added to the HttpContext items.
            //
            // also, it *can* be returned by the container with a PerRequest lifetime, meaning that the
            // container *could* also try to dispose it.
            //
            // all in all, this context may be disposed more than once, but DisposableObject ensures that
            // it is ok and it will be actually disposed only once.
            httpContext.DisposeOnPipelineCompleted(this);

            ObjectCreated = DateTime.Now;
            UmbracoRequestId = Guid.NewGuid();
            HttpContext = httpContext;
            Security = webSecurity;

            // beware - we cannot expect a current user here, so detecting preview mode must be a lazy thing
            _publishedSnapshot = new Lazy<IPublishedSnapshot>(() => publishedSnapshotService.CreatePublishedSnapshot(PreviewToken));

            // set the URLs...
            // NOTE: The request will not be available during app startup so we can only set this to an absolute URL of localhost, this
            // is a work around to being able to access the UmbracoContext during application startup and this will also ensure that people
            // 'could' still generate URLs during startup BUT any domain driven URL generation will not work because it is NOT possible to get
            // the current domain during application startup.
            // see: http://issues.umbraco.org/issue/U4-1890
            //
            OriginalRequestUrl = GetRequestFromContext()?.Url ?? new Uri("http://localhost");
            CleanedUmbracoUrl = UriUtility.UriToUmbraco(OriginalRequestUrl);
            UrlProvider = new UrlProvider(this, umbracoSettings.WebRouting, urlProviders, mediaUrlProviders, variationContextAccessor);
        }

        /// <summary>
        /// This is used internally for performance calculations, the ObjectCreated DateTime is set as soon as this
        /// object is instantiated which in the web site is created during the BeginRequest phase.
        /// We can then determine complete rendering time from that.
        /// </summary>
        internal DateTime ObjectCreated { get; }

        /// <summary>
        /// This is used internally for debugging and also used to define anything required to distinguish this request from another.
        /// </summary>
        internal Guid UmbracoRequestId { get; }

        /// <summary>
        /// Gets the WebSecurity class
        /// </summary>
        public WebSecurity Security { get; }

        /// <summary>
        /// Gets the uri that is handled by ASP.NET after server-side rewriting took place.
        /// </summary>
        internal Uri OriginalRequestUrl { get; }

        /// <summary>
        /// Gets the cleaned up URL that is handled by Umbraco.
        /// </summary>
        /// <remarks>That is, lowercase, no trailing slash after path, no .aspx...</remarks>
        internal Uri CleanedUmbracoUrl { get; }

        /// <summary>
        /// Gets the published snapshot.
        /// </summary>
        public IPublishedSnapshot PublishedSnapshot => _publishedSnapshot.Value;

        /// <summary>
        /// Gets the published content cache.
        /// </summary>
        [Obsolete("Use the Content property.")]
        public IPublishedContentCache ContentCache => PublishedSnapshot.Content;

        /// <summary>
        /// Gets the published content cache.
        /// </summary>
        public IPublishedContentCache Content => PublishedSnapshot.Content;

        /// <summary>
        /// Gets the published media cache.
        /// </summary>
        [Obsolete("Use the Media property.")]
        public IPublishedMediaCache MediaCache => PublishedSnapshot.Media;

        /// <summary>
        /// Gets the published media cache.
        /// </summary>
        public IPublishedMediaCache Media => PublishedSnapshot.Media;

        /// <summary>
        /// Gets the domains cache.
        /// </summary>
        public IDomainCache Domains => PublishedSnapshot.Domains;

        /// <summary>
        /// Boolean value indicating whether the current request is a front-end umbraco request
        /// </summary>
        public bool IsFrontEndUmbracoRequest => PublishedRequest != null;

        /// <summary>
        /// Gets the URL provider.
        /// </summary>
        public UrlProvider UrlProvider { get; }

        /// <summary>
        /// Gets/sets the PublishedRequest object
        /// </summary>
        public PublishedRequest PublishedRequest { get; set; }

        /// <summary>
        /// Exposes the HttpContext for the current request
        /// </summary>
        public HttpContextBase HttpContext { get; }

        /// <summary>
        /// Gets the variation context accessor.
        /// </summary>
        public IVariationContextAccessor VariationContextAccessor { get; }

        /// <summary>
        /// Gets a value indicating whether the request has debugging enabled
        /// </summary>
        /// <value><c>true</c> if this instance is debug; otherwise, <c>false</c>.</value>
        public bool IsDebug
        {
            get
            {
                var request = GetRequestFromContext();
                //NOTE: the request can be null during app startup!
                return GlobalSettings.DebugMode
                    && request != null
                    && (string.IsNullOrEmpty(request["umbdebugshowtrace"]) == false
                        || string.IsNullOrEmpty(request["umbdebug"]) == false
                        || string.IsNullOrEmpty(request.Cookies["UMB-DEBUG"]?.Value) == false);
            }
        }

        /// <summary>
        /// Determines whether the current user is in a preview mode and browsing the site (ie. not in the admin UI)
        /// </summary>
        public bool InPreviewMode
        {
            get
            {
                if (_previewing.HasValue == false) DetectPreviewMode();
                return _previewing ?? false;
            }
            private set => _previewing = value;
        }

        #region Urls

        /// <summary>
        /// Gets the URL of a content identified by its identifier.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="culture"></param>
        /// <returns>The URL for the content.</returns>
        public string Url(int contentId, string culture = null)
        {
            return UrlProvider.GetUrl(contentId, culture: culture);
        }

        /// <summary>
        /// Gets the URL of a content identified by its identifier.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="culture"></param>
        /// <returns>The URL for the content.</returns>
        public string Url(Guid contentId, string culture = null)
        {
            return UrlProvider.GetUrl(contentId, culture: culture);
        }

        /// <summary>
        /// Gets the URL of a content identified by its identifier, in a specified mode.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="culture"></param>
        /// <returns>The URL for the content.</returns>
        public string Url(int contentId, UrlMode mode, string culture = null)
        {
            return UrlProvider.GetUrl(contentId, mode, culture);
        }

        /// <summary>
        /// Gets the URL of a content identified by its identifier, in a specified mode.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="culture"></param>
        /// <returns>The URL for the content.</returns>
        public string Url(Guid contentId, UrlMode mode, string culture = null)
        {
            return UrlProvider.GetUrl(contentId, mode, culture);
        }

        /// <summary>
        /// Gets the absolute URL of a content identified by its identifier.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="culture"></param>
        /// <returns>The absolute URL for the content.</returns>
        [Obsolete("Use the Url() method with UrlMode.Absolute.")]
        public string UrlAbsolute(int contentId, string culture = null)
        {
            return UrlProvider.GetUrl(contentId, UrlMode.Absolute, culture);
        }

        /// <summary>
        /// Gets the absolute URL of a content identified by its identifier.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="culture"></param>
        /// <returns>The absolute URL for the content.</returns>
        [Obsolete("Use the Url() method with UrlMode.Absolute.")]
        public string UrlAbsolute(Guid contentId, string culture = null)
        {
            return UrlProvider.GetUrl(contentId, UrlMode.Absolute, culture);
        }

        #endregion

        private string PreviewToken
        {
            get
            {
                if (_previewing.HasValue == false) DetectPreviewMode();
                return _previewToken;
            }
        }

        private void DetectPreviewMode()
        {
            var request = GetRequestFromContext();
            if (request?.Url != null
                && request.Url.IsBackOfficeRequest(HttpRuntime.AppDomainAppVirtualPath, _globalSettings) == false
                && Security.CurrentUser != null)
            {
                var previewToken = request.GetPreviewCookieValue(); // may be null or empty
                _previewToken = previewToken.IsNullOrWhiteSpace() ? null : previewToken;
            }

            _previewing = _previewToken.IsNullOrWhiteSpace() == false;
        }
        
        // say we render a macro or RTE in a give 'preview' mode that might not be the 'current' one,
        // then due to the way it all works at the moment, the 'current' published snapshot need to be in the proper
        // default 'preview' mode - somehow we have to force it. and that could be recursive.
        internal IDisposable ForcedPreview(bool preview)
        {
            InPreviewMode = preview;
            return PublishedSnapshot.ForcedPreview(preview, orig => InPreviewMode = orig);
        }

        private HttpRequestBase GetRequestFromContext()
        {
            try
            {
                return HttpContext.Request;
            }
            catch (HttpException)
            {
                return null;
            }
        }

        protected override void DisposeResources()
        {
            // DisposableObject ensures that this runs only once

            Security.DisposeIfDisposable();

            // help caches release resources
            // (but don't create caches just to dispose them)
            // context is not multi-threaded
            if (_publishedSnapshot.IsValueCreated)
                _publishedSnapshot.Value.Dispose();
        }
    }
}
