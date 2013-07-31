using System;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using umbraco;
using umbraco.IO;
using umbraco.presentation;
using umbraco.presentation.LiveEditing;
using umbraco.BasePages;
using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic;
using System.Xml;
using umbraco.presentation.preview;
using Examine.Providers;
using Examine;

namespace Umbraco.Web
{

    /// <summary>
    /// Class that encapsulates Umbraco information of a specific HTTP request
    /// </summary>
    public class UmbracoContext : DisposableObject
    {
        private const string HttpContextItemName = "Umbraco.Web.UmbracoContext";
        private static readonly object Locker = new object();

        private bool _replacing;
        private PreviewContent _previewContent;

        /// <summary>
        /// Used if not running in a web application (no real HttpContext)
        /// </summary>
        private static UmbracoContext _umbracoContext;

        /// <summary>
        /// This is a helper method which is called to ensure that the singleton context is created and the nice url and routing
        /// context is created and assigned.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="applicationContext"></param>
        /// <returns>
        /// The Singleton context object
        /// </returns>
        /// <remarks>
        /// This is created in order to standardize the creation of the singleton. Normally it is created during a request
        /// in the UmbracoModule, however this module does not execute during application startup so we need to ensure it
        /// during the startup process as well.
        /// See: http://issues.umbraco.org/issue/U4-1890, http://issues.umbraco.org/issue/U4-1717
        /// </remarks>
        public static UmbracoContext EnsureContext(HttpContextBase httpContext, ApplicationContext applicationContext)
        {
            return EnsureContext(httpContext, applicationContext, false);
        }

        /// <summary>
        /// This is a helper method which is called to ensure that the singleton context is created and the nice url and routing
        /// context is created and assigned.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="applicationContext"></param>
        /// <param name="replaceContext">
        /// if set to true will replace the current singleton with a new one, this is generally only ever used because
        /// during application startup the base url domain will not be available so after app startup we'll replace the current
        /// context with a new one in which we can access the httpcontext.Request object.
        /// </param>
        /// <returns>
        /// The Singleton context object
        /// </returns>
        /// <remarks>
        /// This is created in order to standardize the creation of the singleton. Normally it is created during a request
        /// in the UmbracoModule, however this module does not execute during application startup so we need to ensure it
        /// during the startup process as well.
        /// See: http://issues.umbraco.org/issue/U4-1890, http://issues.umbraco.org/issue/U4-1717
        /// </remarks>
        public static UmbracoContext EnsureContext(HttpContextBase httpContext, ApplicationContext applicationContext, bool replaceContext)
        {
            if (UmbracoContext.Current != null)
            {
                if (!replaceContext)
                    return UmbracoContext.Current;
                UmbracoContext.Current._replacing = true;
            }

            var umbracoContext = new UmbracoContext(
                httpContext,
                applicationContext,
                PublishedCachesResolver.Current.Caches);

            // create the nice urls provider
            // there's one per request because there are some behavior parameters that can be changed
            var urlProvider = new UrlProvider(
                umbracoContext,
                UrlProviderResolver.Current.Providers);

            // create the RoutingContext, and assign
            var routingContext = new RoutingContext(
                umbracoContext,
                ContentFinderResolver.Current.Finders,
                ContentLastChanceFinderResolver.Current.Finder,
                urlProvider);

            //assign the routing context back
            umbracoContext.RoutingContext = routingContext;

            //assign the singleton
            UmbracoContext.Current = umbracoContext;
            return UmbracoContext.Current;
        }

        /// <summary>
        /// Creates a new Umbraco context.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="applicationContext"> </param>
        /// <param name="publishedCaches">The published caches.</param>
        /// <param name="preview">An optional value overriding detection of preview mode.</param>
        internal UmbracoContext(
			HttpContextBase httpContext, 
			ApplicationContext applicationContext,
            IPublishedCaches publishedCaches,
            bool? preview = null)
        {
            if (httpContext == null) throw new ArgumentNullException("httpContext");
            if (applicationContext == null) throw new ArgumentNullException("applicationContext");

    		ObjectCreated = DateTime.Now;
	        UmbracoRequestId = Guid.NewGuid();

            HttpContext = httpContext;            
            Application = applicationContext;
            Security = new WebSecurity();

            ContentCache = publishedCaches.CreateContextualContentCache(this);
            MediaCache = publishedCaches.CreateContextualMediaCache(this);
            InPreviewMode = preview ?? DetectInPreviewModeFromRequest();

			// set the urls...
			//original request url
            //NOTE: The request will not be available during app startup so we can only set this to an absolute URL of localhost, this
            // is a work around to being able to access the UmbracoContext during application startup and this will also ensure that people
            // 'could' still generate URLs during startup BUT any domain driven URL generation will not work because it is NOT possible to get
            // the current domain during application startup.
            // see: http://issues.umbraco.org/issue/U4-1890

            var requestUrl = new Uri("http://localhost");
            var request = GetRequestFromContext();
            if (request != null)
            {
                requestUrl = request.Url;
            }
            this.OriginalRequestUrl = requestUrl;
			//cleaned request url
			this.CleanedUmbracoUrl = UriUtility.UriToUmbraco(this.OriginalRequestUrl);
			
        }

        /// <summary>
        /// Gets the current Umbraco Context.
        /// </summary>
		public static UmbracoContext Current
        {
            get
            {
                //if we have a real context then return the request based object
                if (System.Web.HttpContext.Current != null)
                {
                    return (UmbracoContext)System.Web.HttpContext.Current.Items[HttpContextItemName];
                }

                //return the object if not running in a real HttpContext
                return _umbracoContext;
            }

            internal set
            {
                lock (Locker)
                {
                    //if running in a real HttpContext, this can only be set once
                    if (System.Web.HttpContext.Current != null && Current != null && !Current._replacing)
                    {
                        throw new ApplicationException("The current UmbracoContext can only be set once during a request.");
                    }

                    //if there is an HttpContext, return the item
                    if (System.Web.HttpContext.Current != null)
                    {
                        System.Web.HttpContext.Current.Items[HttpContextItemName] = value;
                    }
                    else
                    {
                        _umbracoContext = value;
                    }
                }
            }
        }

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
        public WebSecurity Security { get; private set; }

	    /// <summary>
	    /// Gets the uri that is handled by ASP.NET after server-side rewriting took place.
	    /// </summary>
		internal Uri OriginalRequestUrl { get; private set; }

		/// <summary>
		/// Gets the cleaned up url that is handled by Umbraco.
		/// </summary>
		/// <remarks>That is, lowercase, no trailing slash after path, no .aspx...</remarks>
		internal Uri CleanedUmbracoUrl { get; private set; }

        /// <summary>
        /// Gets or sets the published content cache.
        /// </summary>
        public ContextualPublishedContentCache ContentCache { get; private set; }

        /// <summary>
        /// Gets or sets the published media cache.
        /// </summary>
        public ContextualPublishedMediaCache MediaCache { get; private set; }

        /// <summary>
		/// Boolean value indicating whether the current request is a front-end umbraco request
		/// </summary>
		public bool IsFrontEndUmbracoRequest
		{
			get { return PublishedContentRequest != null; }
		}

		/// <summary>
		/// A shortcut to the UmbracoContext's RoutingContext's NiceUrlProvider
		/// </summary>
		/// <remarks>
		/// If the RoutingContext is null, this will throw an exception.
		/// </remarks>
    	internal UrlProvider UrlProvider
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
		internal RoutingContext RoutingContext { get; set; }	

		/// <summary>
		/// Gets/sets the PublishedContentRequest object
		/// </summary>
		public PublishedContentRequest PublishedContentRequest { get; set; }	

        /// <summary>
        /// Exposes the HttpContext for the current request
        /// </summary>
        public HttpContextBase HttpContext { get; private set; }

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
                return GlobalSettings.DebugMode && request != null
                    && (!string.IsNullOrEmpty(request["umbdebugshowtrace"]) || !string.IsNullOrEmpty(request["umbdebug"]));
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
        /// Gets the current logged in Umbraco user (editor).
        /// </summary>
        /// <value>The Umbraco user object or null</value>
        public User UmbracoUser
        {
            get
            {
                return Security.CurrentUser;
            }

        }

        /// <summary>
        /// Determines whether the current user is in a preview mode and browsing the site (ie. not in the admin UI)
        /// </summary>
        public bool InPreviewMode { get; private set; }

        private bool DetectInPreviewModeFromRequest()
        {
            var request = GetRequestFromContext();
            if (request == null || request.Url == null)
                return false;

            var currentUrl = request.Url.AbsolutePath;
            // zb-00004 #29956 : refactor cookies names & handling
            return
                StateHelper.Cookies.Preview.HasValue // has preview cookie
                && UmbracoUser != null // has user
                && !currentUrl.StartsWith(Core.IO.IOHelper.ResolveUrl(Core.IO.SystemDirectories.Umbraco)); // is not in admin UI
        }
        
        private HttpRequestBase GetRequestFromContext()
        {
            try
            {
                return HttpContext.Request;
            }
            catch (System.Web.HttpException)
            {
                return null;
            }
        }
        
        protected override void DisposeResources()
        {
            Security.DisposeIfDisposable();
            Security = null;
            _previewContent = null;
            _umbracoContext = null;
            //ensure not to dispose this!
            Application = null;
            ContentCache = null;
            MediaCache = null;     
        }
    }
}