using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using umbraco.BusinessLogic;
using umbraco.presentation.preview;
using GlobalSettings = umbraco.GlobalSettings;
using IOHelper = Umbraco.Core.IO.IOHelper;
using SystemDirectories = Umbraco.Core.IO.SystemDirectories;

namespace Umbraco.Web
{

    /// <summary>
    /// Class that encapsulates Umbraco information of a specific HTTP request
    /// </summary>
    public class UmbracoContext : DisposableObject, IDisposeOnRequestEnd
    {
        private const string HttpContextItemName = "Umbraco.Web.UmbracoContext";
        private static readonly object Locker = new object();

        private bool _replacing;
        private bool? _previewing;
        private readonly Lazy<ContextualPublishedContentCache> _contentCache;
        private readonly Lazy<ContextualPublishedMediaCache> _mediaCache;

        /// <summary>
        /// Used if not running in a web application (no real HttpContext)
        /// </summary>
        [ThreadStatic]
        private static UmbracoContext _umbracoContext;

        #region EnsureContext methods

        #region Obsolete
        [Obsolete("Use the method that specifies IUmbracoSettings instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static UmbracoContext EnsureContext(
            HttpContextBase httpContext,
            ApplicationContext applicationContext,
            WebSecurity webSecurity)
        {
            return EnsureContext(httpContext, applicationContext, webSecurity, false);
        }
        [Obsolete("Use the method that specifies IUmbracoSettings instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static UmbracoContext EnsureContext(
            HttpContextBase httpContext,
            ApplicationContext applicationContext)
        {
            return EnsureContext(httpContext, applicationContext, new WebSecurity(httpContext, applicationContext), false);
        }
        [Obsolete("Use the method that specifies IUmbracoSettings instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static UmbracoContext EnsureContext(
            HttpContextBase httpContext,
            ApplicationContext applicationContext,
            bool replaceContext)
        {
            return EnsureContext(httpContext, applicationContext, new WebSecurity(httpContext, applicationContext), replaceContext);
        }
        [Obsolete("Use the method that specifies IUmbracoSettings instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static UmbracoContext EnsureContext(
            HttpContextBase httpContext,
            ApplicationContext applicationContext,
            WebSecurity webSecurity,
            bool replaceContext)
        {
            return EnsureContext(httpContext, applicationContext, new WebSecurity(httpContext, applicationContext), replaceContext, null);
        }
        [Obsolete("Use the method that specifies IUmbracoSettings instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static UmbracoContext EnsureContext(
            HttpContextBase httpContext,
            ApplicationContext applicationContext,
            WebSecurity webSecurity,
            bool replaceContext,
            bool? preview)
        {
            return EnsureContext(httpContext, applicationContext, webSecurity, UmbracoConfig.For.UmbracoSettings(), replaceContext, preview);
        } 
        #endregion

        /// <summary>
        /// This is a helper method which is called to ensure that the singleton context is created and the nice url and routing
        /// context is created and assigned.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="applicationContext"></param>
        /// <param name="webSecurity"></param>
        /// <param name="umbracoSettings"></param>
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
            WebSecurity webSecurity,
            IUmbracoSettingsSection umbracoSettings)
        {
            return EnsureContext(httpContext, applicationContext, webSecurity, umbracoSettings, false);
        }

        

        /// <summary>
        /// This is a helper method which is called to ensure that the singleton context is created and the nice url and routing
        /// context is created and assigned.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="applicationContext"></param>
        /// <param name="umbracoSettings"></param>
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
            IUmbracoSettingsSection umbracoSettings)
        {
            return EnsureContext(httpContext, applicationContext, new WebSecurity(httpContext, applicationContext), umbracoSettings, false);
        }

       

        /// <summary>
        /// This is a helper method which is called to ensure that the singleton context is created and the nice url and routing
        /// context is created and assigned.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="applicationContext"></param>
        /// <param name="umbracoSettings"></param>
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
        public static UmbracoContext EnsureContext(
            HttpContextBase httpContext,
            ApplicationContext applicationContext,
            IUmbracoSettingsSection umbracoSettings,
            bool replaceContext)
        {
            return EnsureContext(httpContext, applicationContext, new WebSecurity(httpContext, applicationContext), umbracoSettings, replaceContext);
        }

        
        

        /// <summary>
        /// This is a helper method which is called to ensure that the singleton context is created and the nice url and routing
        /// context is created and assigned.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="applicationContext"></param>
        /// <param name="webSecurity"></param>
        /// <param name="umbracoSettings"></param>
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
        public static UmbracoContext EnsureContext(
            HttpContextBase httpContext,
            ApplicationContext applicationContext,
            WebSecurity webSecurity,
            IUmbracoSettingsSection umbracoSettings,
            bool replaceContext)
        {
            return EnsureContext(httpContext, applicationContext, new WebSecurity(httpContext, applicationContext), umbracoSettings, replaceContext, null);
        }

        

        /// <summary>
        /// This is a helper method which is called to ensure that the singleton context is created and the nice url and routing
        /// context is created and assigned.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="applicationContext"></param>
        /// <param name="webSecurity"></param>
        /// <param name="umbracoSettings"></param>
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
            WebSecurity webSecurity,
            IUmbracoSettingsSection umbracoSettings,
            bool replaceContext,
            bool? preview)
        {
            if (UmbracoContext.Current != null)
            {
                if (replaceContext == false)
                    return UmbracoContext.Current;
                UmbracoContext.Current._replacing = true;
            }

            var umbracoContext = new UmbracoContext(
                httpContext,
                applicationContext,
                new Lazy<IPublishedCaches>(() => PublishedCachesResolver.Current.Caches, false),
                webSecurity,
                preview);

            // create the RoutingContext, and assign
            var routingContext = new RoutingContext(
                umbracoContext,
                new Lazy<IEnumerable<IContentFinder>>(() => ContentFinderResolver.Current.Finders),
                new Lazy<IContentFinder>(() => ContentLastChanceFinderResolver.Current.Finder),
                // create the nice urls provider
                // there's one per request because there are some behavior parameters that can be changed
                new Lazy<UrlProvider>(
                    () => new UrlProvider(                        
                        umbracoContext,
                        umbracoSettings.WebRouting,
                        UrlProviderResolver.Current.Providers),
                    false));

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
        /// <param name="webSecurity"></param>
        /// <param name="preview">An optional value overriding detection of preview mode.</param>
        internal UmbracoContext(
            HttpContextBase httpContext,
            ApplicationContext applicationContext,
            IPublishedCaches publishedCaches,
            WebSecurity webSecurity,
            bool? preview = null)
            : this(httpContext, applicationContext, new Lazy<IPublishedCaches>(() => publishedCaches), webSecurity, preview)
        {
        }

        /// <summary>
        /// Creates a new Umbraco context.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="applicationContext"> </param>
        /// <param name="publishedCaches">The published caches.</param>
        /// <param name="webSecurity"></param>
        /// <param name="preview">An optional value overriding detection of preview mode.</param>
        internal UmbracoContext(
			HttpContextBase httpContext, 
			ApplicationContext applicationContext,
            Lazy<IPublishedCaches> publishedCaches,
            WebSecurity webSecurity,
            bool? preview = null)
        {
            //This ensures the dispose method is called when the request terminates, though
            // we also ensure this happens in the Umbraco module because the UmbracoContext is added to the
            // http context items.
            httpContext.DisposeOnPipelineCompleted(this);

            if (httpContext == null) throw new ArgumentNullException("httpContext");
            if (applicationContext == null) throw new ArgumentNullException("applicationContext");

            ObjectCreated = DateTime.Now;
            UmbracoRequestId = Guid.NewGuid();

            HttpContext = httpContext;
            Application = applicationContext;
            Security = webSecurity;

            _contentCache = new Lazy<ContextualPublishedContentCache>(() => publishedCaches.Value.CreateContextualContentCache(this));
            _mediaCache = new Lazy<ContextualPublishedMediaCache>(() => publishedCaches.Value.CreateContextualMediaCache(this));
            _previewing = preview;
            
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
        #endregion

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
        public ContextualPublishedContentCache ContentCache
        {
            get { return _contentCache.Value; }
        }

        /// <summary>
        /// Gets or sets the published media cache.
        /// </summary>
        public ContextualPublishedMediaCache MediaCache
        {
            get { return _mediaCache.Value; }
        }

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
        /// Gets the current logged in Umbraco user (editor).
        /// </summary>
        /// <value>The Umbraco user object or null</value>
        [Obsolete("This should no longer be used since it returns the legacy user object, use The Security.CurrentUser instead to return the proper user object")]
        public User UmbracoUser
        {
            get
            {
                var user = Security.CurrentUser;
                return user == null ? null : new User(user);
            }

        }

        /// <summary>
        /// Determines whether the current user is in a preview mode and browsing the site (ie. not in the admin UI)
        /// </summary>
        /// <remarks>Can be internally set by the RTE macro rendering to render macros in the appropriate mode.</remarks>
        public bool InPreviewMode
        {
            get { return _previewing ?? (_previewing = DetectInPreviewModeFromRequest()).Value; }
			set { _previewing = value; }
        }

        private bool DetectInPreviewModeFromRequest()
        {
            var request = GetRequestFromContext();
            if (request == null || request.Url == null)
                return false;

            var currentUrl = request.Url.AbsolutePath;
            // zb-00004 #29956 : refactor cookies names & handling
            return
                //StateHelper.Cookies.Preview.HasValue // has preview cookie
                HttpContext.Request.HasPreviewCookie()
                && currentUrl.StartsWith(IOHelper.ResolveUrl(SystemDirectories.Umbraco)) == false
                && UmbracoUser != null; // has user
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
            _umbracoContext = null;
            //ensure not to dispose this!
            Application = null;

            //Before we set these to null but in fact these are application lifespan singletons so 
            //there's no reason we need to set them to null and this also caused a problem with packages
            //trying to access the cache properties on RequestEnd.
            //http://issues.umbraco.org/issue/U4-2734
            //http://our.umbraco.org/projects/developer-tools/301-url-tracker/version-2/44327-Issues-with-URL-Tracker-in-614
            //ContentCache = null;
            //MediaCache = null;     
        }
    }
}