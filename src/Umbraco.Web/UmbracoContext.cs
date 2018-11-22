using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Hosting;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Runtime;
using Umbraco.Web.Security;
using LightInject;

namespace Umbraco.Web
{
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

        #region Ensure Context

        ///  <summary>
        ///  Ensures that there is a "current" UmbracoContext.
        ///  </summary>
        /// <param name="umbracoContextAccessor"></param>
        /// <param name="httpContext">An http context.</param>
        /// <param name="publishedSnapshotService">A published snapshot service.</param>
        /// <param name="webSecurity">A security helper.</param>
        /// <param name="umbracoSettings">The umbraco settings.</param>
        /// <param name="urlProviders">Some url providers.</param>
        /// <param name="globalSettings"></param>
        /// <param name="replace">A value indicating whether to replace the existing context.</param>
        ///  <returns>The "current" UmbracoContext.</returns>
        ///  <remarks>
        ///  fixme - this needs to be clarified
        ///
        ///  If <paramref name="replace"/> is true then the "current" UmbracoContext is replaced
        ///  with a new one even if there is one already. See <see cref="WebRuntimeComponent"/>. Has to do with
        ///  creating a context at startup and not being able to access httpContext.Request at that time, so
        ///  the OriginalRequestUrl remains unspecified until <see cref="UmbracoModule"/> replaces the context.
        ///
        ///  This *has* to be done differently!
        ///
        ///  See http://issues.umbraco.org/issue/U4-1890, http://issues.umbraco.org/issue/U4-1717
        ///
        ///  </remarks>
        // used by
        // UmbracoModule BeginRequest (since it's a request it has an UmbracoContext)
        //   in BeginRequest so *late* ie *after* the HttpApplication has started (+ init? check!)
        // WebRuntimeComponent (and I'm not quite sure why)
        // -> because an UmbracoContext seems to be required by UrlProvider to get the "current" published snapshot?
        //    note: at startup not sure we have an HttpContext.Current
        //          at startup not sure we have an httpContext.Request => hard to tell "current" url
        //          should we have a post-boot event of some sort for ppl that *need* ?!
        //          can we have issues w/ routing context?
        // and tests
        // can .ContentRequest be null? of course!
        public static UmbracoContext EnsureContext(
            IUmbracoContextAccessor umbracoContextAccessor,
            HttpContextBase httpContext,
            IPublishedSnapshotService publishedSnapshotService,
            WebSecurity webSecurity,
            IUmbracoSettingsSection umbracoSettings,
            IEnumerable<IUrlProvider> urlProviders,
            IGlobalSettings globalSettings,
            IVariationContextAccessor variationContextAccessor,
            bool replace = false)
        {
            if (umbracoContextAccessor == null) throw new ArgumentNullException(nameof(umbracoContextAccessor));
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (publishedSnapshotService == null) throw new ArgumentNullException(nameof(publishedSnapshotService));
            if (webSecurity == null) throw new ArgumentNullException(nameof(webSecurity));
            if (umbracoSettings == null) throw new ArgumentNullException(nameof(umbracoSettings));
            if (urlProviders == null) throw new ArgumentNullException(nameof(urlProviders));
            if (globalSettings == null) throw new ArgumentNullException(nameof(globalSettings));

            // if there is already a current context, return if not replacing
            var current = umbracoContextAccessor.UmbracoContext;
            if (current != null && replace == false)
                return current;

            // create & assign to accessor, dispose existing if any
            umbracoContextAccessor.UmbracoContext?.Dispose();
            return umbracoContextAccessor.UmbracoContext = new UmbracoContext(httpContext, publishedSnapshotService, webSecurity, umbracoSettings, urlProviders, globalSettings, variationContextAccessor);
        }

        /// <summary>
        /// Gets a disposable object representing the presence of a current UmbracoContext.
        /// </summary>
        /// <remarks>
        /// <para>The disposable object should be used in a using block: using (UmbracoContext.EnsureContext()) { ... }.</para>
        /// <para>If an actual current UmbracoContext is already present, the disposable object is null and this method does nothing.</para>
        /// <para>Otherwise, a temporary, dummy UmbracoContext is created and registered in the accessor. And disposed and removed from the accessor.</para>
        /// </remarks>
        internal static IDisposable EnsureContext() // keep this internal for now!
        {
            if (Composing.Current.UmbracoContext != null) return null;

            var httpContext = new HttpContextWrapper(System.Web.HttpContext.Current ?? new HttpContext(new SimpleWorkerRequest("temp.aspx", "", new StringWriter())));

            return EnsureContext(
                Composing.Current.UmbracoContextAccessor,
                httpContext,
                Composing.Current.PublishedSnapshotService,
                new WebSecurity(httpContext, Composing.Current.Services.UserService, UmbracoConfig.For.GlobalSettings()),
                UmbracoConfig.For.UmbracoSettings(),
                Composing.Current.UrlProviders,
                UmbracoConfig.For.GlobalSettings(),
                Composing.Current.Container.GetInstance<IVariationContextAccessor>(),
                true);

            // when the context will be disposed, it will be removed from the accessor
            // (see DisposeResources)
        }

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

        #endregion

        /// <summary>
        /// Gets the current Umbraco Context.
        /// </summary>
        // note: obsolete, use Current.UmbracoContext... then obsolete Current too, and inject!
        public static UmbracoContext Current => Composing.Current.UmbracoContext;

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
        /// TODO The alternative is to have a IDomainHelperAccessor singleton which is cached per UmbracoContext
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
        /// Gets the current page ID, or <c>null</c> if no page ID is available (e.g. a custom page).
        /// </summary>
        public int? PageId
        {
            get
            {
                try
                {
                    // This was changed but the comments used to refer to
                    // macros in the backoffice not working with this Id
                    // it's probably not a problem any more though. Old comment:
                    // https://github.com/umbraco/Umbraco-CMS/blob/7a615133ff9de84ee667fb7794169af65e2b4d7a/src/Umbraco.Web/UmbracoContext.cs#L256
                    return Current.PublishedRequest.PublishedContent.Id;
                }
                catch
                {
                    return null;
                }
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
                return _previewing.Value;
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

            // reset - important when running outside of http context
            // also takes care of the accessor
            Composing.Current.ClearUmbracoContext();

            // help caches release resources
            // (but don't create caches just to dispose them)
            // context is not multi-threaded
            if (_publishedSnapshot.IsValueCreated)
                _publishedSnapshot.Value.DisposeIfDisposable();
        }
    }
}
