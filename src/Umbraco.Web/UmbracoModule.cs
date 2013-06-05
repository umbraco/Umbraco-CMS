using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Routing;
using umbraco;
using GlobalSettings = Umbraco.Core.Configuration.GlobalSettings;
using UmbracoSettings = Umbraco.Core.Configuration.UmbracoSettings;

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
		void BeginRequest(HttpContextBase httpContext)
		{
            //we need to set the initial url in our ApplicationContext, this is so our keep alive service works and this must
            //exist on a global context because the keep alive service doesn't run in a web context.
            //we are NOT going to put a lock on this because locking will slow down the application and we don't really care
            //if two threads write to this at the exact same time during first page hit.
            //see: http://issues.umbraco.org/issue/U4-2059
            if (ApplicationContext.Current.OriginalRequestUrl.IsNullOrWhiteSpace())
            {
                ApplicationContext.Current.OriginalRequestUrl = string.Format("{0}:{1}{2}", httpContext.Request.ServerVariables["SERVER_NAME"], httpContext.Request.ServerVariables["SERVER_PORT"], IOHelper.ResolveUrl(SystemDirectories.Umbraco));
            }

			// do not process if client-side request
			if (IsClientSideRequest(httpContext.Request.Url))
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
            UmbracoContext.EnsureContext(httpContext, ApplicationContext.Current, true);
		}

		/// <summary>
		/// Processses the Umbraco Request
		/// </summary>
		/// <param name="httpContext"></param>
		void ProcessRequest(HttpContextBase httpContext)
		{
			// do not process if client-side request
			if (IsClientSideRequest(httpContext.Request.Url))
				return;

			if (UmbracoContext.Current == null)
				throw new NullReferenceException("The UmbracoContext.Current is null, ProcessRequest cannot proceed unless there is a current UmbracoContext");
			if (UmbracoContext.Current.RoutingContext == null)
				throw new NullReferenceException("The UmbracoContext.RoutingContext has not been assigned, ProcessRequest cannot proceed unless there is a RoutingContext assigned to the UmbracoContext");

			var umbracoContext = UmbracoContext.Current;		

			// do not process but remap to handler if it is a base rest request
			if (BaseRest.BaseRestHandler.IsBaseRestRequest(umbracoContext.OriginalRequestUrl))
			{
				httpContext.RemapHandler(new BaseRest.BaseRestHandler());
				return;
			}

			// do not process if this request is not a front-end routable page
			if (!EnsureUmbracoRoutablePage(umbracoContext, httpContext))
				return;

			httpContext.Trace.Write("UmbracoModule", "Umbraco request confirmed");

			// ok, process

			var uri = umbracoContext.OriginalRequestUrl;

			// legacy - no idea what this is but does something with the query strings
			LegacyCleanUmbPageFromQueryString(ref uri);

			// instanciate a request a process
			// important to use CleanedUmbracoUrl - lowercase path-only version of the current url
			var docreq = new PublishedContentRequest(umbracoContext.CleanedUmbracoUrl, umbracoContext.RoutingContext);
			docreq.ProcessRequest(httpContext, umbracoContext, 
				docreq2 => RewriteToUmbracoHandler(HttpContext.Current, uri.Query, docreq2.RenderingEngine));
		}

		/// <summary>
		/// Checks if the xml cache file needs to be updated/persisted
		/// </summary>
		/// <param name="httpContext"></param>
		/// <remarks>
		/// TODO: This needs an overhaul, see the error report created here:
		///   https://docs.google.com/document/d/1neGE3q3grB4lVJfgID1keWY2v9JYqf-pw75sxUUJiyo/edit		
		/// </remarks>
		void PersistXmlCache(HttpContextBase httpContext)
		{
			if (content.Instance.IsXmlQueuedForPersistenceToFile)
			{
				content.Instance.RemoveXmlFilePersistenceQueue();
				content.Instance.PersistXmlToFile();
			}
		}

		#endregion

		#region Route helper methods

		/// <summary>
		/// This is a performance tweak to check if this is a .css, .js or .ico file request since
		/// .Net will pass these requests through to the module when in integrated mode.
		/// We want to ignore all of these requests immediately.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		internal bool IsClientSideRequest(Uri url)
		{
			var toIgnore = new[] { ".js", ".css", ".ico" };
			return toIgnore.Any(x => Path.GetExtension(url.LocalPath).InvariantEquals(x));
		}

		/// <summary>
		/// Checks the current request and ensures that it is routable based on the structure of the request and URI
		/// </summary>		
		/// <param name="context"></param>
		/// <param name="httpContext"></param>
		/// <returns></returns>
		internal bool EnsureUmbracoRoutablePage(UmbracoContext context, HttpContextBase httpContext)
		{
			var uri = context.OriginalRequestUrl;

			// ensure this is a document request
			if (!EnsureDocumentRequest(httpContext, uri))
				return false;
			// ensure Umbraco is ready to serve documents
			if (!EnsureIsReady(httpContext, uri))
				return false;
			// ensure Umbraco is properly configured to serve documents
			if (!EnsureIsConfigured(httpContext, uri))
				return false;
            // ensure Umbraco has documents to serve
            if (!EnsureHasContent(context, httpContext))
                return false;

			return true;
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
			if (maybeDoc && GlobalSettings.UseDirectoryUrls)
			{
				int asmxPos = lpath.IndexOf(".asmx/");
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
			if (maybeDoc && GlobalSettings.IsReservedPathOrUrl(lpath, httpContext, RouteTable.Routes))
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
		bool EnsureIsReady(HttpContextBase httpContext, Uri uri)
		{
			var ready = ApplicationContext.Current.IsReady;

			// ensure we are ready
			if (!ready)
			{
				LogHelper.Warn<UmbracoModule>("Umbraco is not ready");

				if (!UmbracoSettings.EnableSplashWhileLoading)
				{
					// let requests pile up and wait for 10s then show the splash anyway
					ready = ApplicationContext.Current.WaitForReady(10 * 1000);
				}

				if (!ready)
				{
					httpContext.Response.StatusCode = 503;

					var bootUrl = UmbracoSettings.BootSplashPage;
					if (string.IsNullOrWhiteSpace(bootUrl))
						bootUrl = "~/config/splashes/booting.aspx";
					httpContext.RewritePath(UriUtility.ToAbsolute(bootUrl) + "?url=" + HttpUtility.UrlEncode(uri.ToString()));

					return false;
				}
			}

			return true;
		}

		// ensures Umbraco has at least one published node
		// if not, rewrites to splash and return false
		// if yes, return true
		bool EnsureHasContent(UmbracoContext context, HttpContextBase httpContext)
		{
			var store = context.RoutingContext.PublishedContentStore;
			if (!store.HasContent(context))
			{
				LogHelper.Warn<UmbracoModule>("Umbraco has no content");

				httpContext.Response.StatusCode = 503;

				var noContentUrl = "~/config/splashes/noNodes.aspx";
				httpContext.RewritePath(UriUtility.ToAbsolute(noContentUrl));

				return false;
			}
			else
			{
				return true;
			}
		}

		// ensures Umbraco is configured
		// if not, redirect to install and return false
		// if yes, return true
		bool EnsureIsConfigured(HttpContextBase httpContext, Uri uri)
		{
			if (!ApplicationContext.Current.IsConfigured)
			{
				LogHelper.Warn<UmbracoModule>("Umbraco is not configured");

				string installPath = UriUtility.ToAbsolute(SystemDirectories.Install);
				string installUrl = string.Format("{0}/default.aspx?redir=true&url={1}", installPath, HttpUtility.UrlEncode(uri.ToString()));
				httpContext.Response.Redirect(installUrl, true);
				return false;
			}
			return true;
		}

		#endregion

		/// <summary>
		/// Rewrites to the correct Umbraco handler, either WebForms or Mvc
		/// </summary>		
		/// <param name="context"></param>
		/// <param name="currentQuery"></param>
		/// <param name="engine"> </param>
		private void RewriteToUmbracoHandler(HttpContext context, string currentQuery, RenderingEngine engine)
		{

			//NOTE: We do not want to use TransferRequest even though many docs say it is better with IIS7, turns out this is
			//not what we need. The purpose of TransferRequest is to ensure that .net processes all of the rules for the newly
			//rewritten url, but this is not what we want!
			// http://forums.iis.net/t/1146511.aspx

			string rewritePath;
			switch (engine)
			{
				case RenderingEngine.Mvc:
					// GlobalSettings.Path has already been through IOHelper.ResolveUrl() so it begins with / and vdir (if any)
					rewritePath = GlobalSettings.Path.TrimEnd(new[] { '/' }) + "/RenderMvc";
					// we rewrite the path to the path of the handler (i.e. default.aspx or /umbraco/RenderMvc )
					context.RewritePath(rewritePath, "", currentQuery.TrimStart(new[] { '?' }), false);

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
					urlRouting.PostResolveRequestCache(new HttpContextWrapper(context));

					break;
				case RenderingEngine.WebForms:
				default:
					rewritePath = "~/default.aspx";
					// rewrite the path to the path of the handler (i.e. default.aspx or /umbraco/RenderMvc )
					context.RewritePath(rewritePath, "", currentQuery.TrimStart(new[] { '?' }), false);

					break;
			}

		}

		#region Legacy

		// "Clean umbPage from querystring, caused by .NET 2.0 default Auth Controls"
		// but really, at the moment I have no idea what this does, and why...
		// SD: I also have no idea what this does, I've googled umbPage and really nothing shows up
		internal static void LegacyCleanUmbPageFromQueryString(ref Uri uri)
		{
			string receivedQuery = uri.Query;
			string path = uri.AbsolutePath;
			string query = null;

			if (receivedQuery.Length > 0)
			{
				// Clean umbPage from querystring, caused by .NET 2.0 default Auth Controls
				if (receivedQuery.IndexOf("umbPage") > 0)
				{
					int ampPos = receivedQuery.IndexOf('&');
					// query contains no ampersand?
					if (ampPos < 0)
					{
						// no ampersand means no original query string
						query = String.Empty;
						// ampersand would occur past then end the of received query
						ampPos = receivedQuery.Length;
					}
					else
					{
						// original query string past ampersand
						query = receivedQuery.Substring(ampPos + 1,
														receivedQuery.Length - ampPos - 1);
					}
					// get umbPage out of query string (9 = "&umbPage".Length() + 1)
					path = receivedQuery.Substring(9, ampPos - 9); //this will fail if there are < 9 characters before the &umbPage query string

					// --added when refactoring--
					uri = uri.Rewrite(path, query);
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
					BeginRequest(new HttpContextWrapper(httpContext));
				};

			app.PostResolveRequestCache += (sender, e) =>
				{
					var httpContext = ((HttpApplication)sender).Context;
					ProcessRequest(new HttpContextWrapper(httpContext));
				};

			// used to check if the xml cache file needs to be updated/persisted
			app.PostRequestHandlerExecute += (sender, e) =>
				{
					var httpContext = ((HttpApplication)sender).Context;
					PersistXmlCache(new HttpContextWrapper(httpContext));
				};

			app.EndRequest += (sender, args) =>
				{
					var httpContext = ((HttpApplication)sender).Context;					
					if (UmbracoContext.Current != null && UmbracoContext.Current.IsFrontEndUmbracoRequest)
					{
						//write the trace output for diagnostics at the end of the request
						httpContext.Trace.Write("UmbracoModule", "Umbraco request completed");	
						LogHelper.Debug<UmbracoModule>("Total milliseconds for umbraco request to process: " + DateTime.Now.Subtract(UmbracoContext.Current.ObjectCreated).TotalMilliseconds);
					}

					DisposeHttpContextItems(httpContext);
				};

            //disable asp.net headers (security)
		    app.PreSendRequestHeaders += (sender, args) =>
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
		}

		public void Dispose()
		{

		}

		#endregion

		/// <summary>
		/// Any object that is in the HttpContext.Items collection that is IDisposable will get disposed on the end of the request
		/// </summary>
		/// <param name="http"></param>
		private static void DisposeHttpContextItems(HttpContext http)
		{
			foreach(var i in http.Items)
			{
				i.DisposeIfDisposable();
			}
		}
	}
}
