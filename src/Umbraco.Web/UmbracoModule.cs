using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Web.Routing;
using umbraco;
using umbraco.IO;
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
			// do not process if client-side request
			if (IsClientSideRequest(httpContext.Request.Url))
				return;

			// ok, process

			// create the LegacyRequestInitializer
			// and initialize legacy stuff
			var legacyRequestInitializer = new LegacyRequestInitializer(httpContext.Request.Url, httpContext);
			legacyRequestInitializer.InitializeRequest();

			// create the UmbracoContext singleton, one per request, and assign
			var umbracoContext = new UmbracoContext(
				httpContext,
				ApplicationContext.Current,
				RoutesCacheResolver.Current.RoutesCache);
			UmbracoContext.Current = umbracoContext;

			// create the nice urls provider
			var niceUrls = new NiceUrlProvider(PublishedContentStoreResolver.Current.PublishedContentStore, umbracoContext);

			// create the RoutingContext, and assign
			var routingContext = new RoutingContext(
				umbracoContext,
				DocumentLookupsResolver.Current.DocumentLookups,
				LastChanceLookupResolver.Current.LastChanceLookup,
				PublishedContentStoreResolver.Current.PublishedContentStore,
				niceUrls);
			umbracoContext.RoutingContext = routingContext;
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

			var umbracoContext = UmbracoContext.Current;
			var routingContext = umbracoContext.RoutingContext;

			// do not process but remap to handler if it is a base rest request
			if (BaseRest.BaseRestHandler.IsBaseRestRequest(umbracoContext.RequestUrl))
			{
				httpContext.RemapHandler(new BaseRest.BaseRestHandler());
				return;
			}

			// do not process if this request is not a front-end routable page
			if (!EnsureUmbracoRoutablePage(umbracoContext, httpContext))
				return;

			// ok, process

			var uri = umbracoContext.RequestUrl;

			// legacy - no idea what this is
			LegacyCleanUmbPageFromQueryString(ref uri);

			// create the new document request since we're rendering a document on the front-end
			var docreq = new DocumentRequest(
				umbracoContext.UmbracoUrl, //very important to use this url! it is the path only lowercased version of the current URL. 
				routingContext);
			//assign the document request to the umbraco context now that we know its a front end request
			umbracoContext.DocumentRequest = docreq;

			// note - at that point the original legacy module did something do handle IIS custom 404 errors
			//   ie pages looking like /anything.aspx?404;/path/to/document - I guess the reason was to support
			//   "directory urls" without having to do wildcard mapping to ASP.NET on old IIS. This is a pain
			//   to maintain and probably not used anymore - removed as of 06/2012. @zpqrtbnk.
			//
			//   to trigger Umbraco's not-found, one should configure IIS and/or ASP.NET custom 404 errors
			//   so that they point to a non-existing page eg /redirect-404.aspx
			//   TODO: SD: We need more information on this for when we release 4.10.0 as I'm not sure what this means.

			//create the searcher
			var searcher = new DocumentRequestBuilder(docreq);
			//find domain
			searcher.LookupDomain();
			// redirect if it has been flagged
			if (docreq.IsRedirect)
				httpContext.Response.Redirect(docreq.RedirectUrl, true);
			//set the culture on the thread
			Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = docreq.Culture;
			//find the document, found will be true if the doc request has found BOTH a node and a template
			// though currently we don't use this value.
			var found = searcher.LookupDocument();
			//this could be called in the LookupDocument method, but I've just put it here for clarity.
			searcher.DetermineRenderingEngine();

			//TODO: here we should launch an event so that people can modify the doc request to do whatever they want.

			// redirect if it has been flagged
			if (docreq.IsRedirect)
				httpContext.Response.Redirect(docreq.RedirectUrl, true);

			// handle 404
			if (docreq.Is404)
			{
				httpContext.Response.StatusCode = 404;

				if (!docreq.HasNode)
				{
					httpContext.RemapHandler(new DocumentNotFoundHandler());
					return;
				}

				// else we have a document to render
				// not having a template is ok here, MVC will take care of it
			}

			// just be safe - should never ever happen
			if (!docreq.HasNode)
				throw new Exception("No document to render.");

			// render even though we might have no template
			// to give MVC a chance to hijack routes
			// pass off to our handlers (mvc or webforms)

			// assign the legacy page back to the docrequest
			// handlers like default.aspx will want it
			docreq.UmbracoPage = new page(docreq);

			// these two are used by many legacy objects
			httpContext.Items["pageID"] = docreq.DocumentId;
			httpContext.Items["pageElements"] = docreq.UmbracoPage.Elements;

			RewriteToUmbracoHandler(HttpContext.Current, uri.Query, docreq.RenderingEngine);
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
			var uri = context.RequestUrl;

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
            if (!EnsureHasContent(httpContext))
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
			if (maybeDoc && GlobalSettings.IsReservedPathOrUrl(lpath))
				maybeDoc = false;

			if (!maybeDoc)
			{
				LogHelper.Warn<UmbracoModule>("Not a document");
			}

			return maybeDoc;
		}

		// ensures Umbraco is ready to handle requests
		// if not, set status to 503 and transfer request, and return false
		// if yes, return true
		bool EnsureIsReady(HttpContextBase httpContext, Uri uri)
		{
			// ensure we are ready
			if (!ApplicationContext.Current.IsReady)
			{
				LogHelper.Warn<UmbracoModule>("Umbraco is not ready");

				httpContext.Response.StatusCode = 503;

				// fixme - default.aspx has to be ready for RequestContext.DocumentRequest==null
				// fixme - in fact we should transfer to an empty html page...
				var bootUrl = UriUtility.ToAbsolute(UmbracoSettings.BootSplashPage);

				if (UmbracoSettings.EnableSplashWhileLoading) // legacy - should go
				{
					var configPath = UriUtility.ToAbsolute(SystemDirectories.Config);
					bootUrl = string.Format("{0}/splashes/booting.aspx?url={1}", configPath, HttpUtility.UrlEncode(uri.ToString()));
					// fixme ?orgurl=... ?retry=...
				}

				httpContext.RewritePath(bootUrl);

				return false;
			}

			return true;
		}

        // ensures Umbraco has at least one published node
        // if not, rewrites to splash and return false
        // if yes, return true
        bool EnsureHasContent(HttpContextBase httpContext)
        {
            var context = UmbracoContext.Current;
            var store = context.RoutingContext.PublishedContentStore;
            if (!store.HasContent(context))
            {
                LogHelper.Warn<UmbracoModule>("Umbraco has not content");

                var noContentUrl = UriUtility.ToAbsolute(UmbracoSettings.NoContentSplashPage);
                httpContext.RewritePath(noContentUrl);
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
		/// <param name="isMvc"> </param>
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
					//the Path is normally ~/umbraco but we need to remove the start ~/ of it and if someone modifies this
					//then we should be rendering the MVC stuff in that location.
					rewritePath = "~/"
						+ GlobalSettings.Path.TrimStart(new[] { '~', '/' }).TrimEnd(new[] { '/' })
						+ "/RenderMvc" + currentQuery;
					//First we rewrite the path to the path of the handler (i.e. default.aspx or /umbraco/RenderMvc )
					context.RewritePath(rewritePath, "", currentQuery.TrimStart(new[] { '?' }), false);

					//if it is MVC we need to do something special, we are not using TransferRequest as this will 
					//require us to rewrite the path with query strings and then reparse the query strings, this would 
					//also mean that we need to handle IIS 7 vs pre-IIS 7 differently. Instead we are just going to create
					//an instance of the UrlRoutingModule and call it's PostResolveRequestCache method. This does:
					// * Looks up the route based on the new rewritten URL
					// * Creates the RequestContext with all route parameters and then executes the correct handler that matches the route
					//we could have re-written this functionality, but the code inside the PostResolveRequestCache is exactly what we want.					
					var urlRouting = new UrlRoutingModule();
					urlRouting.PostResolveRequestCache(new HttpContextWrapper(context));

					break;
				case RenderingEngine.WebForms:
				default:
					rewritePath = "~/default.aspx" + currentQuery;
					//First we rewrite the path to the path of the handler (i.e. default.aspx or /umbraco/RenderMvc )
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

			// todo: initialize request errors handler
		}

		public void Dispose()
		{

		}

		#endregion

	}
}
