using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Runtime;
using Umbraco.Web.Security;

namespace Umbraco.Web
{
    public interface IUmbracoContextFactory
    {
        UmbracoContextReference EnsureUmbracoContext(HttpContextBase httpContext = null);
    }

    public class UmbracoContextReference : IDisposable
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private bool _disposed;

        internal UmbracoContextReference(UmbracoContext umbracoContext, bool isRoot, IUmbracoContextAccessor umbracoContextAccessor)
        {
            UmbracoContext = umbracoContext;
            IsRoot = isRoot;

            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public UmbracoContext UmbracoContext { get; }

        public bool IsRoot { get; }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            if (IsRoot)
            {
                UmbracoContext.Dispose();
                _umbracoContextAccessor.UmbracoContext = null;
            }

            GC.SuppressFinalize(this);
        }
    }

    public class UmbracoContextFactory : IUmbracoContextFactory
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly IDefaultCultureAccessor _defaultCultureAccessor;

        private readonly IUmbracoSettingsSection _umbracoSettings;
        private readonly IGlobalSettings _globalSettings;
        private readonly IEnumerable<IUrlProvider> _urlProviders;
        private readonly IUserService _userService;

        public UmbracoContextFactory(IUmbracoContextAccessor umbracoContextAccessor, IPublishedSnapshotService publishedSnapshotService, IVariationContextAccessor variationContextAccessor, IDefaultCultureAccessor defaultCultureAccessor, IUmbracoSettingsSection umbracoSettings, IGlobalSettings globalSettings, IEnumerable<IUrlProvider> urlProviders, IUserService userService)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _publishedSnapshotService = publishedSnapshotService ?? throw new ArgumentNullException(nameof(publishedSnapshotService));
            _variationContextAccessor = variationContextAccessor ?? throw new ArgumentNullException(nameof(variationContextAccessor));
            _defaultCultureAccessor = defaultCultureAccessor ?? throw new ArgumentNullException(nameof(defaultCultureAccessor));

            _umbracoSettings = umbracoSettings ?? throw new ArgumentNullException(nameof(umbracoSettings));
            _globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));
            _urlProviders = urlProviders ?? throw new ArgumentNullException(nameof(urlProviders));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public UmbracoContext CreateUmbracoContext(HttpContextBase httpContext)
        {
            // make sure we have a variation context
            if (_variationContextAccessor.VariationContext == null)
                _variationContextAccessor.VariationContext = new VariationContext(_defaultCultureAccessor.DefaultCulture);

            var webSecurity = new WebSecurity(httpContext, _userService, _globalSettings);
            return new UmbracoContext(httpContext, _publishedSnapshotService, webSecurity, _umbracoSettings, _urlProviders, _globalSettings, _variationContextAccessor);
        }

        public UmbracoContext EnsureUmbracoContext___(HttpContextBase httpContext)
        {
            var currentUmbracoContext = _umbracoContextAccessor.UmbracoContext;
            if (currentUmbracoContext != null)
            {
                currentUmbracoContext.Dispose();
                _umbracoContextAccessor.UmbracoContext = null;
            }

            var umbracoContext = CreateUmbracoContext(httpContext);
            _umbracoContextAccessor.UmbracoContext = umbracoContext;
            return umbracoContext;
        }

        public UmbracoContextReference EnsureUmbracoContext(HttpContextBase httpContext = null)
        {
            var currentUmbracoContext = _umbracoContextAccessor.UmbracoContext;
            if (currentUmbracoContext != null) return new UmbracoContextReference(currentUmbracoContext, false, _umbracoContextAccessor);

            httpContext = httpContext ?? new HttpContextWrapper(HttpContext.Current ?? new HttpContext(new SimpleWorkerRequest("nul.aspx", "", NulWriter.Instance)));

            var umbracoContext = CreateUmbracoContext(httpContext);
            _umbracoContextAccessor.UmbracoContext = umbracoContext;

            return new UmbracoContextReference(umbracoContext, true, _umbracoContextAccessor);
        }

        private class NulWriter : TextWriter
        {
            private NulWriter()
            { }

            public static NulWriter Instance { get; } = new NulWriter();

            public override Encoding Encoding => Encoding.UTF8;
        }
    }

    /// <summary>
    /// Class that encapsulates Umbraco information of a specific HTTP request
    /// </summary>
    public class UmbracoContext : DisposableObject, IDisposeOnRequestEnd
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly Lazy<IPublishedSnapshot> _publishedSnapshot;
        private DomainHelper _domainHelper;
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
            IGlobalSettings globalSettings,
            IVariationContextAccessor variationContextAccessor)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (publishedSnapshotService == null) throw new ArgumentNullException(nameof(publishedSnapshotService));
            if (webSecurity == null) throw new ArgumentNullException(nameof(webSecurity));
            if (umbracoSettings == null) throw new ArgumentNullException(nameof(umbracoSettings));
            if (urlProviders == null) throw new ArgumentNullException(nameof(urlProviders));
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

            // set the urls...
            // NOTE: The request will not be available during app startup so we can only set this to an absolute URL of localhost, this
            // is a work around to being able to access the UmbracoContext during application startup and this will also ensure that people
            // 'could' still generate URLs during startup BUT any domain driven URL generation will not work because it is NOT possible to get
            // the current domain during application startup.
            // see: http://issues.umbraco.org/issue/U4-1890
            //
            OriginalRequestUrl = GetRequestFromContext()?.Url ?? new Uri("http://localhost");
            CleanedUmbracoUrl = UriUtility.UriToUmbraco(OriginalRequestUrl);
            UrlProvider = new UrlProvider(this, umbracoSettings.WebRouting, urlProviders, variationContextAccessor);
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
        /// Gets the cleaned up url that is handled by Umbraco.
        /// </summary>
        /// <remarks>That is, lowercase, no trailing slash after path, no .aspx...</remarks>
        internal Uri CleanedUmbracoUrl { get; }

        /// <summary>
        /// Gets the published snapshot.
        /// </summary>
        public IPublishedSnapshot PublishedSnapshot => _publishedSnapshot.Value;

        // for unit tests
        internal bool HasPublishedSnapshot => _publishedSnapshot.IsValueCreated;

        /// <summary>
        /// Gets the published content cache.
        /// </summary>
        public IPublishedContentCache ContentCache => PublishedSnapshot.Content;

        /// <summary>
        /// Gets the published media cache.
        /// </summary>
        public IPublishedMediaCache MediaCache => PublishedSnapshot.Media;

        /// <summary>
        /// Boolean value indicating whether the current request is a front-end umbraco request
        /// </summary>
        public bool IsFrontEndUmbracoRequest => PublishedRequest != null;

        /// <summary>
        /// Gets the url provider.
        /// </summary>
        public UrlProvider UrlProvider { get; }

        /// <summary>
        /// Gets/sets the PublishedContentRequest object
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
        /// Creates and caches an instance of a DomainHelper
        /// </summary>
        /// <remarks>
        /// We keep creating new instances of DomainHelper, it would be better if we didn't have to do that so instead we can
        /// have one attached to the UmbracoContext. This method accepts an external ISiteDomainHelper otherwise the UmbracoContext
        /// ctor will have to have another parameter added only for this one method which is annoying and doesn't make a ton of sense
        /// since the UmbracoContext itself doesn't use this.
        ///
        /// TODO: The alternative is to have a IDomainHelperAccessor singleton which is cached per UmbracoContext
        /// </remarks>
        internal DomainHelper GetDomainHelper(ISiteDomainHelper siteDomainHelper)
            => _domainHelper ?? (_domainHelper = new DomainHelper(PublishedSnapshot.Domains, siteDomainHelper));

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
                        || string.IsNullOrEmpty(request["umbdebug"]) == false);
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
