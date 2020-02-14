using System;
using System.Collections.Generic;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Composing;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Web
{
    /// <summary>
    /// Class that encapsulates Umbraco information of a specific HTTP request
    /// </summary>
    public class UmbracoContext : DisposableObjectSlim, IDisposeOnRequestEnd, IUmbracoContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGlobalSettings _globalSettings;
        private readonly IIOHelper _ioHelper;
        private readonly IPublishedUrlProvider _publishedUrlProvider;
        private readonly Lazy<IPublishedSnapshot> _publishedSnapshot;
        private string _previewToken;
        private bool? _previewing;

        // initializes a new instance of the UmbracoContext class
        // internal for unit tests
        // otherwise it's used by EnsureContext above
        // warn: does *not* manage setting any IUmbracoContextAccessor
        internal UmbracoContext(IHttpContextAccessor httpContextAccessor,
            IPublishedSnapshotService publishedSnapshotService,
            IWebSecurity webSecurity,
            IGlobalSettings globalSettings,
            IVariationContextAccessor variationContextAccessor,
            IIOHelper ioHelper,
            IPublishedUrlProvider publishedUrlProvider)
        {
            if (httpContextAccessor == null) throw new ArgumentNullException(nameof(httpContextAccessor));
            if (publishedSnapshotService == null) throw new ArgumentNullException(nameof(publishedSnapshotService));
            if (webSecurity == null) throw new ArgumentNullException(nameof(webSecurity));
            VariationContextAccessor = variationContextAccessor ??  throw new ArgumentNullException(nameof(variationContextAccessor));
            _httpContextAccessor = httpContextAccessor;
            _globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));
            _ioHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));
            _publishedUrlProvider = publishedUrlProvider;

            // ensure that this instance is disposed when the request terminates, though we *also* ensure
            // this happens in the Umbraco module since the UmbracoCOntext is added to the HttpContext items.
            //
            // also, it *can* be returned by the container with a PerRequest lifetime, meaning that the
            // container *could* also try to dispose it.
            //
            // all in all, this context may be disposed more than once, but DisposableObject ensures that
            // it is ok and it will be actually disposed only once.
            httpContextAccessor.HttpContext.DisposeOnPipelineCompleted(this);

            ObjectCreated = DateTime.Now;
            UmbracoRequestId = Guid.NewGuid();
            Security = webSecurity;

            // beware - we cannot expect a current user here, so detecting preview mode must be a lazy thing
            _publishedSnapshot = new Lazy<IPublishedSnapshot>(() => publishedSnapshotService.CreatePublishedSnapshot(PreviewToken));

            // set the urls...
            // NOTE: The request will not be available during app startup so we can only set this to an absolute URL of localhost, this
            // is a work around to being able to access the UmbracoContext during application startup and this will also ensure that people
            // 'could' still generate URLs during startup BUT any domain driven URL generation will not work because it is NOT possible to get
            // the current domain during application startup.
            // see: http://issues.umbraco.org/issue/U4-1890
            //
            OriginalRequestUrl = GetRequestFromContext()?.Url ?? new Uri("http://localhost");
            CleanedUmbracoUrl = UriUtility.UriToUmbraco(OriginalRequestUrl);
        }

        /// <summary>
        /// This is used internally for performance calculations, the ObjectCreated DateTime is set as soon as this
        /// object is instantiated which in the web site is created during the BeginRequest phase.
        /// We can then determine complete rendering time from that.
        /// </summary>
        public DateTime ObjectCreated { get; }

        /// <summary>
        /// This is used internally for debugging and also used to define anything required to distinguish this request from another.
        /// </summary>
        public Guid UmbracoRequestId { get; }

        /// <summary>
        /// Gets the WebSecurity class
        /// </summary>
        public IWebSecurity Security { get; }

        /// <summary>
        /// Gets the uri that is handled by ASP.NET after server-side rewriting took place.
        /// </summary>
        public Uri OriginalRequestUrl { get; }

        /// <summary>
        /// Gets the cleaned up url that is handled by Umbraco.
        /// </summary>
        /// <remarks>That is, lowercase, no trailing slash after path, no .aspx...</remarks>
        public Uri CleanedUmbracoUrl { get; }

        /// <summary>
        /// Gets the published snapshot.
        /// </summary>
        public IPublishedSnapshot PublishedSnapshot => _publishedSnapshot.Value;

        /// <summary>
        /// Gets the published content cache.
        /// </summary>
        public IPublishedContentCache Content => PublishedSnapshot.Content;

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
        /// Gets/sets the PublishedRequest object
        /// </summary>
        public IPublishedRequest PublishedRequest { get; set; }

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
                return Current.RuntimeState.Debug
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
        /// Gets the url of a content identified by its identifier.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="culture"></param>
        /// <returns>The url for the content.</returns>
        public string Url(int contentId, string culture = null)
        {
            return _publishedUrlProvider.GetUrl(contentId, culture: culture);
        }

        /// <summary>
        /// Gets the url of a content identified by its identifier.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="culture"></param>
        /// <returns>The url for the content.</returns>
        public string Url(Guid contentId, string culture = null)
        {
            return _publishedUrlProvider.GetUrl(contentId, culture: culture);
        }

        /// <summary>
        /// Gets the url of a content identified by its identifier, in a specified mode.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="culture"></param>
        /// <returns>The url for the content.</returns>
        public string Url(int contentId, UrlMode mode, string culture = null)
        {
            return _publishedUrlProvider.GetUrl(contentId, mode, culture);
        }

        /// <summary>
        /// Gets the url of a content identified by its identifier, in a specified mode.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="culture"></param>
        /// <returns>The url for the content.</returns>
        public string Url(Guid contentId, UrlMode mode, string culture = null)
        {
            return _publishedUrlProvider.GetUrl(contentId, mode, culture);
        }

        #endregion

        public string PreviewToken
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
                && request.Url.IsBackOfficeRequest(HttpRuntime.AppDomainAppVirtualPath, _globalSettings, _ioHelper) == false
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
        public IDisposable ForcedPreview(bool preview)
        {
            InPreviewMode = preview;
            return PublishedSnapshot.ForcedPreview(preview, orig => InPreviewMode = orig);
        }

        private HttpRequestBase GetRequestFromContext()
        {
            try
            {
                return _httpContextAccessor.HttpContext.Request;
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
