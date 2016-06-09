using System;
using System.Collections.Generic;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Web
{
    /// <summary>
    /// Class that encapsulates Umbraco information of a specific HTTP request
    /// </summary>
    public class UmbracoContext : DisposableObject, IDisposeOnRequestEnd
    {
        private bool? _previewing;
        private readonly Lazy<IFacade> _facade;

        #region Ensure Context

        /// <summary>
        /// This is a helper method which is called to ensure that the singleton context is created
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="applicationContext"></param>
        /// <param name="facadeService"></param>
        /// <param name="webSecurity"></param>
        /// <param name="umbracoSettings"></param>
        /// <param name="urlProviders"></param>
        /// <param name="replaceContext">
        /// if set to true will replace the current singleton with a new one, this is generally only ever used because
        /// during application startup the base url domain will not be available so after app startup we'll replace the current
        /// context with a new one in which we can access the httpcontext.Request object.
        /// </param>
        /// <param name="preview"></param>
        /// <returns>
        /// The Singleton context object
        /// </returns>
        /// <remarks>
        /// This is created in order to standardize the creation of the singleton. Normally it is created during a request
        /// in the UmbracoModule, however this module does not execute during application startup so we need to ensure it
        /// during the startup process as well.
        /// See: http://issues.umbraco.org/issue/U4-1890, http://issues.umbraco.org/issue/U4-1717
        /// </remarks>
        public static UmbracoContext EnsureContext(
            HttpContextBase httpContext,
            ApplicationContext applicationContext,
            IFacadeService facadeService,
            WebSecurity webSecurity,
            IUmbracoSettingsSection umbracoSettings,
            IEnumerable<IUrlProvider> urlProviders,
            bool replaceContext,
            bool? preview = null)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (applicationContext == null) throw new ArgumentNullException(nameof(applicationContext));
            if (webSecurity == null) throw new ArgumentNullException(nameof(webSecurity));
            if (umbracoSettings == null) throw new ArgumentNullException(nameof(umbracoSettings));
            if (urlProviders == null) throw new ArgumentNullException(nameof(urlProviders));

            // if there is already a current context, return if not replacing
            var umbracoContext = Web.Current.UmbracoContext;
            if (umbracoContext != null && replaceContext == false)
                return umbracoContext;

            // create, assign the singleton, and return
            umbracoContext = CreateContext(httpContext, applicationContext, facadeService, webSecurity, umbracoSettings, urlProviders, preview);
            Web.Current.SetUmbracoContext(umbracoContext, replaceContext);
            return umbracoContext;
        }

        /// <summary>
        /// Creates a standalone UmbracoContext instance
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="applicationContext"></param>
        /// <param name="facadeService"></param>
        /// <param name="webSecurity"></param>
        /// <param name="umbracoSettings"></param>
        /// <param name="urlProviders"></param>
        /// <param name="preview"></param>
        /// <returns>
        /// A new instance of UmbracoContext
        /// </returns>
        internal static UmbracoContext CreateContext(
            HttpContextBase httpContext,
            ApplicationContext applicationContext,
            IFacadeService facadeService,
            WebSecurity webSecurity,
            IUmbracoSettingsSection umbracoSettings,
            IEnumerable<IUrlProvider> urlProviders,
            bool? preview = null)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (applicationContext == null) throw new ArgumentNullException(nameof(applicationContext));
            if (webSecurity == null) throw new ArgumentNullException(nameof(webSecurity));
            if (umbracoSettings == null) throw new ArgumentNullException(nameof(umbracoSettings));
            if (urlProviders == null) throw new ArgumentNullException(nameof(urlProviders));

            // create the context
            var umbracoContext = new UmbracoContext(
                httpContext,
                applicationContext,
                facadeService,
                webSecurity,
                preview);

            // create and assign the RoutingContext,
            // note the circular dependency here
            umbracoContext.RoutingContext = new RoutingContext(
                umbracoContext,

                //TODO: Until the new cache is done we can't really expose these to override/mock
                new Lazy<IEnumerable<IContentFinder>>(() => ContentFinderResolver.Current.Finders),
                new Lazy<IContentFinder>(() => ContentLastChanceFinderResolver.Current.Finder),

                // create the nice urls provider
                // there's one per request because there are some behavior parameters that can be changed
                new Lazy<UrlProvider>(
                    () => new UrlProvider(
                        umbracoContext,
                        umbracoSettings.WebRouting,
                        urlProviders),
                    false));

            return umbracoContext;
        }

        /// <param name="httpContext">An HttpContext.</param>
        /// <param name="applicationContext">An Umbraco application context.</param>
        /// <param name="facadeService">A facade service.</param>
        /// <param name="webSecurity">A web security.</param>
        /// <param name="preview">An optional value overriding detection of preview mode.</param>
        private UmbracoContext(
			HttpContextBase httpContext,
			ApplicationContext applicationContext,
            IFacadeService facadeService,
            WebSecurity webSecurity,
            bool? preview = null)
        {
            // ensure that this instance is disposed when the request terminates,
            // though we *also* ensure this happens in the Umbraco module since the
            // UmbracoCOntext is added to the HttpContext items.
            httpContext.DisposeOnPipelineCompleted(this);

            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (applicationContext == null) throw new ArgumentNullException(nameof(applicationContext));

            ObjectCreated = DateTime.Now;
            UmbracoRequestId = Guid.NewGuid();

            HttpContext = httpContext;
            Application = applicationContext;
            Security = webSecurity;

            _facade = new Lazy<IFacade>(() => facadeService.CreateFacade(PreviewToken));
            _previewing = preview; // fixme are we ignoring this entirely?!

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

        #endregion

        /// <summary>
        /// Gets the current Umbraco Context.
        /// </summary>
        // note: obsolete, use Current.UmbracoContext... then obsolete Current too, and inject!
        public static UmbracoContext Current => Web.Current.UmbracoContext;

        /// <summary>
		/// This is used internally for performance calculations, the ObjectCreated DateTime is set as soon as this
		/// object is instantiated which in the web site is created during the BeginRequest phase.
		/// We can then determine complete rendering time from that.
		/// </summary>
		internal DateTime ObjectCreated { get; private set; }

		/// <summary>
		/// This is used internally for debugging and also used to define anything required to distinguish this request from another.
		/// </summary>
		internal Guid UmbracoRequestId { get; private set; }

        /// <summary>
        /// Gets the current ApplicationContext
        /// </summary>
        public ApplicationContext Application { get; private set; }

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
		internal Uri CleanedUmbracoUrl { get; private set; }

        /// <summary>
        /// Gets the facade.
        /// </summary>
        public IFacade Facade => _facade.Value;

        // for unit tests
        internal bool HasFacade => _facade.IsValueCreated;

        /// <summary>
        /// Gets the published content cache.
        /// </summary>
        public IPublishedContentCache ContentCache => Facade.ContentCache;

        /// <summary>
        /// Gets the published media cache.
        /// </summary>
        public IPublishedMediaCache MediaCache => Facade.MediaCache;

        /// <summary>
        /// Boolean value indicating whether the current request is a front-end umbraco request
        /// </summary>
        public bool IsFrontEndUmbracoRequest => PublishedContentRequest != null;

        /// <summary>
		/// A shortcut to the UmbracoContext's RoutingContext's NiceUrlProvider
		/// </summary>
		/// <remarks>
		/// If the RoutingContext is null, this will throw an exception.
		/// </remarks>
    	public UrlProvider UrlProvider
    	{
    		get
    		{
    			if (RoutingContext == null)
					throw new InvalidOperationException("Cannot access the UrlProvider when the UmbracoContext's RoutingContext is null");
    			return RoutingContext.UrlProvider;
    		}
    	}

		/// <summary>
		/// Gets/sets the RoutingContext object
		/// </summary>
		public RoutingContext RoutingContext { get; internal set; }

		/// <summary>
		/// Gets/sets the PublishedContentRequest object
		/// </summary>
		public PublishedContentRequest PublishedContentRequest { get; set; }

        /// <summary>
        /// Exposes the HttpContext for the current request
        /// </summary>
        public HttpContextBase HttpContext { get; }

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
            // TODO - this is dirty old legacy tricks, we should clean it up at some point
            // also, what is a "custom page" and when should this be either null, or different
            // from PublishedContentRequest.PublishedContent.Id ??
            // SD: Have found out it can be different when rendering macro contents in the back office, but really youshould just be able
            // to pass a page id to the macro renderer instead but due to all the legacy bits that's real difficult.
            get
            {
                try
                {
                    //TODO: this should be done with a wrapper: http://issues.umbraco.org/issue/U4-61
                    return int.Parse(HttpContext.Items["pageID"].ToString());
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
        /// <remarks>Can be internally set by the RTE macro rendering to render macros in the appropriate mode.</remarks>
        // fixme - that's bad, RTE macros should then create their own facade?! they don't have a preview token!
        public bool InPreviewMode
        {
            get { return _previewing ?? (_previewing = (PreviewToken.IsNullOrWhiteSpace() == false)).Value; }
            set { _previewing = value; }
        }

        private string PreviewToken
        {
            get
            {
                var request = GetRequestFromContext();
                if (request?.Url == null)
                    return null;

                if (request.Url.IsBackOfficeRequest(HttpRuntime.AppDomainAppVirtualPath)) return null;
                if (Security.CurrentUser == null) return null;

                var previewToken = request.GetPreviewCookieValue(); // may be null or empty
                return previewToken.IsNullOrWhiteSpace() ? null : previewToken;
            }
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
            Security.DisposeIfDisposable();

            //If not running in a web ctx, ensure the thread based instance is nulled
            Web.Current.SetUmbracoContext(null, true);

            // help caches release resources
            // (but don't create caches just to dispose them)
            // context is not multi-threaded
            if (_facade.IsValueCreated)
                _facade.Value.DisposeIfDisposable();
        }
    }
}