using System;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.UmbracoContext
{
    /// <summary>
    /// Class that encapsulates Umbraco information of a specific HTTP request
    /// </summary>
    public class UmbracoContext : DisposableObjectSlim, IDisposeOnRequestEnd, IUmbracoContext
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly UriUtility _uriUtility;
        private readonly ICookieManager _cookieManager;
        private readonly IRequestAccessor _requestAccessor;
        private readonly Lazy<IPublishedSnapshot> _publishedSnapshot;
        private string _previewToken;
        private bool? _previewing;
        private readonly IBackOfficeSecurity _backofficeSecurity;
        private readonly UmbracoRequestPaths _umbracoRequestPaths;
        private Uri _originalRequestUrl;
        private Uri _cleanedUmbracoUrl;

        // initializes a new instance of the UmbracoContext class
        // internal for unit tests
        // otherwise it's used by EnsureContext above
        // warn: does *not* manage setting any IUmbracoContextAccessor
        internal UmbracoContext(
            IPublishedSnapshotService publishedSnapshotService,
            IBackOfficeSecurity backofficeSecurity,
            UmbracoRequestPaths umbracoRequestPaths,
            IHostingEnvironment hostingEnvironment,
            IVariationContextAccessor variationContextAccessor,
            UriUtility uriUtility,
            ICookieManager cookieManager,
            IRequestAccessor requestAccessor)
        {
            if (publishedSnapshotService == null)
            {
                throw new ArgumentNullException(nameof(publishedSnapshotService));
            }

            VariationContextAccessor = variationContextAccessor ?? throw new ArgumentNullException(nameof(variationContextAccessor));
            _uriUtility = uriUtility;
            _hostingEnvironment = hostingEnvironment;
            _cookieManager = cookieManager;
            _requestAccessor = requestAccessor;

            ObjectCreated = DateTime.Now;
            UmbracoRequestId = Guid.NewGuid();
            _backofficeSecurity = backofficeSecurity;
            _umbracoRequestPaths = umbracoRequestPaths;

            // beware - we cannot expect a current user here, so detecting preview mode must be a lazy thing
            _publishedSnapshot = new Lazy<IPublishedSnapshot>(() => publishedSnapshotService.CreatePublishedSnapshot(PreviewToken));
        }

        /// <inheritdoc/>
        public DateTime ObjectCreated { get; }

        /// <summary>
        /// Gets the context Id
        /// </summary>
        /// <remarks>
        /// Used internally for debugging and also used to define anything required to distinguish this request from another.
        /// </remarks>
        internal Guid UmbracoRequestId { get; }

        /// <inheritdoc/>
        // set the urls lazily, no need to allocate until they are needed...
        // NOTE: The request will not be available during app startup so we can only set this to an absolute URL of localhost, this
        // is a work around to being able to access the UmbracoContext during application startup and this will also ensure that people
        // 'could' still generate URLs during startup BUT any domain driven URL generation will not work because it is NOT possible to get
        // the current domain during application startup.
        // see: http://issues.umbraco.org/issue/U4-1890
        public Uri OriginalRequestUrl => _originalRequestUrl ?? (_originalRequestUrl = _requestAccessor.GetRequestUrl() ?? new Uri("http://localhost"));

        /// <inheritdoc/>
        // set the urls lazily, no need to allocate until they are needed...
        public Uri CleanedUmbracoUrl => _cleanedUmbracoUrl ?? (_cleanedUmbracoUrl = _uriUtility.UriToUmbraco(OriginalRequestUrl));

        /// <inheritdoc/>
        public IPublishedSnapshot PublishedSnapshot => _publishedSnapshot.Value;

        /// <inheritdoc/>
        public IPublishedContentCache Content => PublishedSnapshot.Content;

        /// <inheritdoc/>
        public IPublishedMediaCache Media => PublishedSnapshot.Media;

        /// <inheritdoc/>
        public IDomainCache Domains => PublishedSnapshot.Domains;

        /// <inheritdoc/>
        public IPublishedRequest PublishedRequest { get; set; }

        /// <inheritdoc/>
        public IVariationContextAccessor VariationContextAccessor { get; }

        /// <inheritdoc/>
        public bool IsDebug => // NOTE: the request can be null during app startup!
                _hostingEnvironment.IsDebugMode
                       && (string.IsNullOrEmpty(_requestAccessor.GetRequestValue("umbdebugshowtrace")) == false
                           || string.IsNullOrEmpty(_requestAccessor.GetRequestValue("umbdebug")) == false
                           || string.IsNullOrEmpty(_cookieManager.GetCookieValue("UMB-DEBUG")) == false);

        /// <inheritdoc/>
        public bool InPreviewMode
        {
            get
            {
                if (_previewing.HasValue == false)
                {
                    DetectPreviewMode();
                }

                return _previewing ?? false;
            }
            private set => _previewing = value;
        }

        internal string PreviewToken
        {
            get
            {
                if (_previewing.HasValue == false)
                {
                    DetectPreviewMode();
                }

                return _previewToken;
            }
        }

        private void DetectPreviewMode()
        {
            Uri requestUrl = _requestAccessor.GetRequestUrl();
            if (requestUrl != null
                && _umbracoRequestPaths.IsBackOfficeRequest(requestUrl.AbsolutePath) == false
                && _backofficeSecurity?.CurrentUser != null)
            {
                var previewToken = _cookieManager.GetCookieValue(Core.Constants.Web.PreviewCookieName); // may be null or empty
                _previewToken = previewToken.IsNullOrWhiteSpace() ? null : previewToken;
            }

            _previewing = _previewToken.IsNullOrWhiteSpace() == false;
        }

        /// <inheritdoc/>
        public IDisposable ForcedPreview(bool preview)
        {
            // say we render a macro or RTE in a give 'preview' mode that might not be the 'current' one,
            // then due to the way it all works at the moment, the 'current' published snapshot need to be in the proper
            // default 'preview' mode - somehow we have to force it. and that could be recursive.
            InPreviewMode = preview;
            return PublishedSnapshot.ForcedPreview(preview, orig => InPreviewMode = orig);
        }

        /// <inheritdoc/>
        protected override void DisposeResources()
        {
            // DisposableObject ensures that this runs only once

            // help caches release resources
            // (but don't create caches just to dispose them)
            // context is not multi-threaded
            if (_publishedSnapshot.IsValueCreated)
            {
                _publishedSnapshot.Value.Dispose();
            }
        }
    }
}
