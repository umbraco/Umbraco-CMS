using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Security;
using Umbraco.Web.Editors;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using umbraco;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;
using ObjectExtensions = Umbraco.Core.ObjectExtensions;
using RenderingEngine = Umbraco.Core.RenderingEngine;

namespace Umbraco.Web
{
	// also look at IOHelper.ResolveUrlsFromTextString - nightmarish?!

	// context.RewritePath supports ~/ or else must begin with /vdir
	//  Request.RawUrl is still there
	// response.Redirect does?! always remap to /vdir?!

	public class UmbracoModule : IHttpModule
	{
		#region HttpModule event handlers

		/// <summary>
		/// Begins to process a request.
		/// </summary>
		/// <param name="httpContext"></param>
        static void BeginRequest(HttpContextBase httpContext)
		{

            //we need to set the initial url in our ApplicationContext, this is so our keep alive service works and this must
            //exist on a global context because the keep alive service doesn't run in a web context.
            //we are NOT going to put a lock on this because locking will slow down the application and we don't really care
            //if two threads write to this at the exact same time during first page hit.
            //see: http://issues.umbraco.org/issue/U4-2059
            if (ApplicationContext.Current.OriginalRequestUrl.IsNullOrWhiteSpace())
            {
                // If (HTTP and SSL not required) or (HTTPS and SSL required), use ports from request to configure OriginalRequestUrl.
                // Otherwise, user may need to set baseUrl manually per http://our.umbraco.org/documentation/Using-Umbraco/Config-files/umbracoSettings/#ScheduledTasks if non-standard ports used.
                if ((!httpContext.Request.IsSecureConnection && !GlobalSettings.UseSSL) || (httpContext.Request.IsSecureConnection && GlobalSettings.UseSSL))
                {
                    // Use port from request.
                    ApplicationContext.Current.OriginalRequestUrl = string.Format("{0}:{1}{2}", httpContext.Request.ServerVariables["SERVER_NAME"], httpContext.Request.ServerVariables["SERVER_PORT"], IOHelper.ResolveUrl(SystemDirectories.Umbraco));
                }
                else
                {
                    // Omit port entirely.
                    ApplicationContext.Current.OriginalRequestUrl = string.Format("{0}{1}", httpContext.Request.ServerVariables["SERVER_NAME"], IOHelper.ResolveUrl(SystemDirectories.Umbraco));
                }

                LogHelper.Info<UmbracoModule>("Setting OriginalRequestUrl: " + ApplicationContext.Current.OriginalRequestUrl);
            }

			// do not process if client-side request
			if (httpContext.Request.Url.IsClientSideRequest())
				return;

			//write the trace output for diagnostics at the end of the request
			httpContext.Trace.Write("UmbracoModule", "Umbraco request begins");

            // ok, process

            // create the LegacyRequestInitializer
            // and initialize legacy stuff
            var legacyRequestInitializer = new LegacyRequestInitializer(httpContext.Request.Url, httpContext);
            legacyRequestInitializer.InitializeRequest();

            // create the UmbracoContext singleton, one per request, and assign
            // NOTE: we assign 'true' to ensure the context is replaced if it is already set (i.e. during app startup)            
            UmbracoContext.EnsureContext(
                httpContext, 
                ApplicationContext.Current, 
                new WebSecurity(httpContext, ApplicationContext.Current), 
                true);    
		}

		/// <summary>
		/// Processses the Umbraco Request
		/// </summary>
		/// <param name="httpContext"></param>
		/// <remarks>
		/// 
		/// This will check if we are trying to route to the default back office page (i.e. ~/Umbraco/ or ~/Umbraco or ~/Umbraco/Default )
		/// and ensure that the MVC handler executes for that. This is required because the route for /Umbraco will never execute because 
        /// files/folders exist there and we cannot set the RouteCollection.RouteExistingFiles = true since that will muck a lot of other things up.
        /// So we handle it here and explicitly execute the MVC controller.
		/// 
		/// </remarks>
		void ProcessRequest(HttpContextBase httpContext)
		{
			// do not process if client-side request
			if (httpContext.Request.Url.IsClientSideRequest())
				return;

			if (UmbracoContext.Current == null)
				throw new InvalidOperationException("The UmbracoContext.Current is null, ProcessRequest cannot proceed unless there is a current UmbracoContext");
			if (UmbracoContext.Current.RoutingContext == null)
				throw new InvalidOperationException("The UmbracoContext.RoutingContext has not been assigned, ProcessRequest cannot proceed unless there is a RoutingContext assigned to the UmbracoContext");

			var umbracoContext = UmbracoContext.Current;

            //re-write for the default back office path
            if (httpContext.Request.Url.IsDefaultBackOfficeRequest())
            {
                if (EnsureIsConfigured(httpContext, umbracoContext.OriginalRequestUrl))
                {
                    RewriteToBackOfficeHandler(httpContext);                    
                }
                return;
            }

			// do not process but remap to handler if it is a base rest request
			if (BaseRest.BaseRestHandler.IsBaseRestRequest(umbracoContext.OriginalRequestUrl))
			{
				httpContext.RemapHandler(new BaseRest.BaseRestHandler());
				return;
			}

			// do not process if this request is not a front-end routable page
		    var isRoutableAttempt = EnsureUmbracoRoutablePage(umbracoContext, httpContext);
            //raise event here
            OnRouteAttempt(new RoutableAttemptEventArgs(isRoutableAttempt.Result, umbracoContext, httpContext));
            if (!isRoutableAttempt.Success)
			{
                return;
			}
				

			httpContext.Trace.Write("UmbracoModule", "Umbraco request confirmed");

			// ok, process

			// note: requestModule.UmbracoRewrite also did some stripping of &umbPage
			// from the querystring... that was in v3.x to fix some issues with pre-forms
			// auth. Paul Sterling confirmed in jan. 2013 that we can get rid of it.

			// instanciate, prepare and process the published content request
			// important to use CleanedUmbracoUrl - lowercase path-only version of the current url
			var pcr = new PublishedContentRequest(umbracoContext.CleanedUmbracoUrl, umbracoContext.RoutingContext);
			umbracoContext.PublishedContentRequest = pcr;
			pcr.Prepare();

            // HandleHttpResponseStatus returns a value indicating that the request should
            // not be processed any further, eg because it has been redirect. then, exit.
            if (HandleHttpResponseStatus(httpContext, pcr))
		        return;

            if (!pcr.HasPublishedContent)
				httpContext.RemapHandler(new PublishedContentNotFoundHandler());
			else
				RewriteToUmbracoHandler(httpContext, pcr);
		}

		#endregion

		#region Methods

        /// <summary>
        /// Determines if we should authenticate the request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="originalRequestUrl"></param>
        /// <returns></returns>
        /// <remarks>
        /// We auth the request when:
        /// * it is a back office request
        /// * it is an installer request
        /// * it is a /base request
        /// * it is a preview request
        /// </remarks>
        internal static bool ShouldAuthenticateRequest(HttpRequestBase request, Uri originalRequestUrl)
        {
            if (//check back office
                request.Url.IsBackOfficeRequest(HttpRuntime.AppDomainAppVirtualPath)
                //check installer
                || request.Url.IsInstallerRequest()
                //detect in preview
                || (request.HasPreviewCookie() && request.Url != null && request.Url.AbsolutePath.StartsWith(IOHelper.ResolveUrl(SystemDirectories.Umbraco)) == false)
                //check for base
                || BaseRest.BaseRestHandler.IsBaseRestRequest(originalRequestUrl))
            {
                return true;
            }
            return false;
        }

        private static readonly ConcurrentHashSet<string> IgnoreTicketRenewUrls = new ConcurrentHashSet<string>(); 
        /// <summary>
        /// Determines if the authentication ticket should be renewed with a new timeout
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        /// <remarks>
        /// We do not want to renew the ticket when we are checking for the user's remaining timeout unless -
        /// UmbracoConfig.For.UmbracoSettings().Security.KeepUserLoggedIn == true
        /// </remarks>
        internal static bool ShouldIgnoreTicketRenew(Uri url, HttpContextBase httpContext)
        {
            //this setting will renew the ticket for all requests.
            if (UmbracoConfig.For.UmbracoSettings().Security.KeepUserLoggedIn)
            {
                return false;
            }

            //initialize the ignore ticket urls - we don't need to lock this, it's concurrent and a hashset
            // we don't want to have to gen the url each request so this will speed things up a teeny bit.
            if (IgnoreTicketRenewUrls.Any() == false)
            {                
                var urlHelper = new UrlHelper(new RequestContext(httpContext, new RouteData()));
                var checkSessionUrl = urlHelper.GetUmbracoApiService<AuthenticationController>(controller => controller.GetRemainingTimeoutSeconds());
                IgnoreTicketRenewUrls.Add(checkSessionUrl);
            }

            if (IgnoreTicketRenewUrls.Any(x => url.AbsolutePath.StartsWith(x)))
            {
                return true;
            }

            return false;            
        }

		/// <summary>
		/// Checks the current request and ensures that it is routable based on the structure of the request and URI
		/// </summary>		
		/// <param name="context"></param>
		/// <param name="httpContext"></param>
		/// <returns></returns>
        internal Attempt<EnsureRoutableOutcome> EnsureUmbracoRoutablePage(UmbracoContext context, HttpContextBase httpContext)
		{
			var uri = context.OriginalRequestUrl;

		    var reason = EnsureRoutableOutcome.IsRoutable;

			// ensure this is a document request
			if (!EnsureDocumentRequest(httpContext, uri))
			{
			    reason = EnsureRoutableOutcome.NotDocumentRequest;
			}
			// ensure Umbraco is ready to serve documents
			else if (!EnsureIsReady(httpContext, uri))
			{
			    reason = EnsureRoutableOutcome.NotReady;
			}                
			// ensure Umbraco is properly configured to serve documents
			else if (!EnsureIsConfigured(httpContext, uri))
            {
                reason = EnsureRoutableOutcome.NotConfigured;
            }                
            // ensure Umbraco has documents to serve
            else if (!EnsureHasContent(context, httpContext))
            {
                reason = EnsureRoutableOutcome.NoContent;
            }

            return Attempt.If(reason == EnsureRoutableOutcome.IsRoutable, reason);
		}

		/// <summary>
		/// Ensures that the request is a document request (i.e. one that the module should handle)
		/// </summary>
		/// <param name="httpContext"></param>
		/// <param name="uri"></param>
		/// <returns></returns>
		bool EnsureDocumentRequest(HttpContextBase httpContext, Uri uri)
		{
			var maybeDoc = true;
			var lpath = uri.AbsolutePath.ToLowerInvariant();

			// handle directory-urls used for asmx
			// legacy - what's the point really?
			if (/*maybeDoc &&*/ GlobalSettings.UseDirectoryUrls)
			{
				int asmxPos = lpath.IndexOf(".asmx/", StringComparison.OrdinalIgnoreCase);
				if (asmxPos >= 0)
				{
					// use uri.AbsolutePath, not path, 'cos path has been lowercased
					httpContext.RewritePath(uri.AbsolutePath.Substring(0, asmxPos + 5), // filePath
						uri.AbsolutePath.Substring(asmxPos + 5), // pathInfo
						uri.Query.TrimStart('?'));
					maybeDoc = false;
				}
			}

			// a document request should be
			// /foo/bar/nil
			// /foo/bar/nil/
			// /foo/bar/nil.aspx
			// where /foo is not a reserved path

			// if the path contains an extension that is not .aspx
			// then it cannot be a document request
			if (maybeDoc && lpath.Contains('.') && !lpath.EndsWith(".aspx"))
				maybeDoc = false;

			// at that point, either we have no extension, or it is .aspx

			// if the path is reserved then it cannot be a document request
            if (maybeDoc && GlobalSettings.IsReservedPathOrUrl(lpath, httpContext, _combinedRouteCollection.Value))
				maybeDoc = false;

			//NOTE: No need to warn, plus if we do we should log the document, as this message doesn't really tell us anything :)
			//if (!maybeDoc)
			//{
			//	LogHelper.Warn<UmbracoModule>("Not a document");
			//}

			return maybeDoc;
		}

		// ensures Umbraco is ready to handle requests
		// if not, set status to 503 and transfer request, and return false
		// if yes, return true
	    static bool EnsureIsReady(HttpContextBase httpContext, Uri uri)
		{
			var ready = ApplicationContext.Current.IsReady;

			// ensure we are ready
			if (!ready)
			{
				LogHelper.Warn<UmbracoModule>("Umbraco is not ready");

                if (UmbracoConfig.For.UmbracoSettings().Content.EnableSplashWhileLoading == false)
				{
					// let requests pile up and wait for 10s then show the splash anyway
					ready = ApplicationContext.Current.WaitForReady(10 * 1000);
				}

				if (!ready)
				{
					httpContext.Response.StatusCode = 503;

                    var bootUrl = "~/config/splashes/booting.aspx";
					
					httpContext.RewritePath(UriUtility.ToAbsolute(bootUrl) + "?url=" + HttpUtility.UrlEncode(uri.ToString()));

					return false;
				}
			}

			return true;
		}

		// ensures Umbraco has at least one published node
		// if not, rewrites to splash and return false
		// if yes, return true
	    private static bool EnsureHasContent(UmbracoContext context, HttpContextBase httpContext)
		{
            if (context.ContentCache.HasContent())
		        return true;

            LogHelper.Warn<UmbracoModule>("Umbraco has no content");
            
			const string noContentUrl = "~/config/splashes/noNodes.aspx";
			httpContext.RewritePath(UriUtility.ToAbsolute(noContentUrl));

			return false;
		}

	    private bool _notConfiguredReported;

		// ensures Umbraco is configured
		// if not, redirect to install and return false
		// if yes, return true
	    private bool EnsureIsConfigured(HttpContextBase httpContext, Uri uri)
	    {
	        if (ApplicationContext.Current.IsConfigured)
	            return true;

	        if (_notConfiguredReported)
	        {
                // remember it's been reported so we don't flood the log
                // no thread-safety so there may be a few log entries, doesn't matter
                _notConfiguredReported = true;
                LogHelper.Warn<UmbracoModule>("Umbraco is not configured");
            }

			var installPath = UriUtility.ToAbsolute(SystemDirectories.Install);
			var installUrl = string.Format("{0}/?redir=true&url={1}", installPath, HttpUtility.UrlEncode(uri.ToString()));
			httpContext.Response.Redirect(installUrl, true);
			return false;
		}

        // returns a value indicating whether redirection took place and the request has
        // been completed - because we don't want to Response.End() here to terminate
        // everything properly.
        internal static bool HandleHttpResponseStatus(HttpContextBase context, PublishedContentRequest pcr)
        {
            var end = false;
            var response = context.Response;

            LogHelper.Debug<UmbracoModule>("Response status: Redirect={0}, Is404={1}, StatusCode={2}",
                () => pcr.IsRedirect ? (pcr.IsRedirectPermanent ? "permanent" : "redirect") : "none",
                () => pcr.Is404 ? "true" : "false", () => pcr.ResponseStatusCode);

            if (pcr.IsRedirect)
            {
                if (pcr.IsRedirectPermanent)
                    response.RedirectPermanent(pcr.RedirectUrl, false); // do not end response
                else
                    response.Redirect(pcr.RedirectUrl, false); // do not end response
                end = true;
            }
            else if (pcr.Is404)
            {
                response.StatusCode = 404;
                response.TrySkipIisCustomErrors = UmbracoConfig.For.UmbracoSettings().WebRouting.TrySkipIisCustomErrors;

                if (response.TrySkipIisCustomErrors == false)
                    LogHelper.Warn<UmbracoModule>("Status code is 404 yet TrySkipIisCustomErrors is false - IIS will take over.");          
            }

            if (pcr.ResponseStatusCode > 0)
            {
                // set status code -- even for redirects
                response.StatusCode = pcr.ResponseStatusCode;
                response.StatusDescription = pcr.ResponseStatusDescription;
            }
            //if (pcr.IsRedirect)
            //    response.End(); // end response -- kills the thread and does not return!

            if (pcr.IsRedirect)
            {
                response.Flush();
                // bypass everything and directly execute EndRequest event -- but returns
                context.ApplicationInstance.CompleteRequest();
                // though some say that .CompleteRequest() does not properly shutdown the response
                // and the request will hang until the whole code has run... would need to test?
                LogHelper.Debug<UmbracoModule>("Response status: redirecting, complete request now.");
            }

            return end;
        }

        /// <summary>
        /// Rewrites to the default back office page.
        /// </summary>
        /// <param name="context"></param>
        private static void RewriteToBackOfficeHandler(HttpContextBase context)
        {
            // GlobalSettings.Path has already been through IOHelper.ResolveUrl() so it begins with / and vdir (if any)
            var rewritePath = GlobalSettings.Path.TrimEnd(new[] { '/' }) + "/Default";
            // rewrite the path to the path of the handler (i.e. /umbraco/RenderMvc)
            context.RewritePath(rewritePath, "", "", false);

            //if it is MVC we need to do something special, we are not using TransferRequest as this will 
            //require us to rewrite the path with query strings and then reparse the query strings, this would 
            //also mean that we need to handle IIS 7 vs pre-IIS 7 differently. Instead we are just going to create
            //an instance of the UrlRoutingModule and call it's PostResolveRequestCache method. This does:
            // * Looks up the route based on the new rewritten URL
            // * Creates the RequestContext with all route parameters and then executes the correct handler that matches the route
            //we also cannot re-create this functionality because the setter for the HttpContext.Request.RequestContext is internal
            //so really, this is pretty much the only way without using Server.TransferRequest and if we did that, we'd have to rethink
            //a bunch of things!
            var urlRouting = new UrlRoutingModule();
            urlRouting.PostResolveRequestCache(context);
        }

        /// <summary>
		/// Rewrites to the Umbraco handler - we always send the request via our MVC rendering engine, this will deal with
		/// requests destined for webforms.
        /// </summary>		
        /// <param name="context"></param>
        /// <param name="pcr"> </param>
        private static void RewriteToUmbracoHandler(HttpContextBase context, PublishedContentRequest pcr)
        {
            // NOTE: we do not want to use TransferRequest even though many docs say it is better with IIS7, turns out this is
            // not what we need. The purpose of TransferRequest is to ensure that .net processes all of the rules for the newly
            // rewritten url, but this is not what we want!
            // read: http://forums.iis.net/t/1146511.aspx

			var query = pcr.Uri.Query.TrimStart(new[] { '?' });

            // GlobalSettings.Path has already been through IOHelper.ResolveUrl() so it begins with / and vdir (if any)
            var rewritePath = GlobalSettings.Path.TrimEnd(new[] { '/' }) + "/RenderMvc";
            // rewrite the path to the path of the handler (i.e. /umbraco/RenderMvc)
            context.RewritePath(rewritePath, "", query, false);

            //if it is MVC we need to do something special, we are not using TransferRequest as this will 
            //require us to rewrite the path with query strings and then reparse the query strings, this would 
            //also mean that we need to handle IIS 7 vs pre-IIS 7 differently. Instead we are just going to create
            //an instance of the UrlRoutingModule and call it's PostResolveRequestCache method. This does:
            // * Looks up the route based on the new rewritten URL
            // * Creates the RequestContext with all route parameters and then executes the correct handler that matches the route
            //we also cannot re-create this functionality because the setter for the HttpContext.Request.RequestContext is internal
            //so really, this is pretty much the only way without using Server.TransferRequest and if we did that, we'd have to rethink
            //a bunch of things!
            var urlRouting = new UrlRoutingModule();
            urlRouting.PostResolveRequestCache(context);
        }

       
	    /// <summary>
        /// Any object that is in the HttpContext.Items collection that is IDisposable will get disposed on the end of the request
        /// </summary>
        /// <param name="http"></param>
        private static void DisposeHttpContextItems(HttpContext http)
        {
            // do not process if client-side request
            if (http.Request.Url.IsClientSideRequest())
                return;

            //get a list of keys to dispose
            var keys = new HashSet<object>();            
            foreach (DictionaryEntry i in http.Items)
            {
                if (i.Value is IDisposeOnRequestEnd || i.Key is IDisposeOnRequestEnd)
                {
                    keys.Add(i.Key);
                }
            }
            //dispose each item and key that was found as disposable.
            foreach (var k in keys)
            {
                try
                {
                    http.Items[k].DisposeIfDisposable();
                }
                catch (Exception ex)
                {
                    LogHelper.Error<UmbracoModule>("Could not dispose item with key " + k, ex);
                }
                try
                {
                    k.DisposeIfDisposable();
                }
                catch (Exception ex)
                {
                    LogHelper.Error<UmbracoModule>("Could not dispose item key " + k, ex);
                }
            }
        }

		#endregion

		#region IHttpModule

		/// <summary>
		/// Initialize the module,  this will trigger for each new application 
		/// and there may be more than 1 application per application domain
		/// </summary>
		/// <param name="app"></param>
		public void Init(HttpApplication app)
		{
			app.BeginRequest += (sender, e) =>
				{
					var httpContext = ((HttpApplication)sender).Context;
				    LogHelper.Debug<UmbracoModule>("Begin request: {0}.", () => httpContext.Request.Url);
                    BeginRequest(new HttpContextWrapper(httpContext));
				};

            //disable asp.net headers (security)
            // This is the correct place to modify headers according to MS: 
            // https://our.umbraco.org/forum/umbraco-7/using-umbraco-7/65241-Heap-error-from-header-manipulation?p=0#comment220889
		    app.PostReleaseRequestState += (sender, args) =>
		    {
                var httpContext = ((HttpApplication)sender).Context;
                try
                {
                    httpContext.Response.Headers.Remove("Server");
                    //this doesn't normally work since IIS sets it but we'll keep it here anyways.
                    httpContext.Response.Headers.Remove("X-Powered-By");
                }
                catch (PlatformNotSupportedException ex)
                {
                    // can't remove headers this way on IIS6 or cassini.
                }
		    };

            app.PostResolveRequestCache += (sender, e) =>
				{
					var httpContext = ((HttpApplication)sender).Context;
					ProcessRequest(new HttpContextWrapper(httpContext));
				};

			app.EndRequest += (sender, args) =>
				{
					var httpContext = ((HttpApplication)sender).Context;					
					if (UmbracoContext.Current != null && UmbracoContext.Current.IsFrontEndUmbracoRequest)
					{
						LogHelper.Debug<UmbracoModule>(
                            "Total milliseconds for umbraco request to process: {0}", () => DateTime.Now.Subtract(UmbracoContext.Current.ObjectCreated).TotalMilliseconds);
					}

                    OnEndRequest(new EventArgs());

					DisposeHttpContextItems(httpContext);
				};

		}

		public void Dispose()
		{

		}

		#endregion

        #region Events
        internal static event EventHandler<RoutableAttemptEventArgs> RouteAttempt;
        private void OnRouteAttempt(RoutableAttemptEventArgs args)
        {
            if (RouteAttempt != null)
                RouteAttempt(this, args);
        }

        internal static event EventHandler<EventArgs> EndRequest;
        private void OnEndRequest(EventArgs args)
        {
            if (EndRequest != null)
                EndRequest(this, args);
        } 
        #endregion


        /// <summary>
        /// This is used to be passed into the GlobalSettings.IsReservedPathOrUrl and will include some 'fake' routes
        /// used to determine if a path is reserved.
        /// </summary>
        /// <remarks>
        /// This is basically used to reserve paths dynamically
        /// </remarks>
        private readonly Lazy<RouteCollection> _combinedRouteCollection = new Lazy<RouteCollection>(() =>
        {
            var allRoutes = new RouteCollection();
            foreach (var route in RouteTable.Routes)
            {
                allRoutes.Add(route);
            }
            foreach (var reservedPath in ReservedPaths)
            {
                try
                {
                    allRoutes.Add("_umbreserved_" + reservedPath.ReplaceNonAlphanumericChars(""),
                                new Route(reservedPath.TrimStart('/'), new StopRoutingHandler()));
                }
                catch (Exception ex)
                {
                    LogHelper.Error<UmbracoModule>("Could not add reserved path route", ex);
                }
            }

            return allRoutes;
        }); 

        /// <summary>
        /// This is used internally to track any registered callback paths for Identity providers. If the request path matches
        /// any of the registered paths, then the module will let the request keep executing
        /// </summary>
        internal static readonly ConcurrentHashSet<string> ReservedPaths = new ConcurrentHashSet<string>();
	}
}
