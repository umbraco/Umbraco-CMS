using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
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
		/// <summary>
		/// Checks if the current request should process the request as a front-end umbraco request, if this is tru
		/// it then creates the DocumentRequest object, finds the document, domain and culture and stores this back 
		/// to the UmbracoContext
		/// </summary>
		/// <param name="httpContext"></param>
		/// <param name="umbracoContext"> </param>
		/// <param name="uri"> </param>
		internal void AssignDocumentRequest(
			HttpContextBase httpContext, 
			UmbracoContext umbracoContext,
			Uri uri)
		{
			if (httpContext == null) throw new ArgumentNullException("httpContext");
			if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
			if (umbracoContext.RoutingContext == null) throw new ArgumentNullException("umbracoContext.RoutingContext");

			//Create a document request since we are rendering a document on the front-end

			// create the new document request which will cleanup the uri once and for all
			var docreq = new DocumentRequest(uri, umbracoContext);
			//assign the routing context to the umbraco context
			umbracoContext.DocumentRequest = docreq;

			// note - at that point the original legacy module did something do handle IIS custom 404 errors
			//   ie pages looking like /anything.aspx?404;/path/to/document - I guess the reason was to support
			//   "directory urls" without having to do wildcard mapping to ASP.NET on old IIS. This is a pain
			//   to maintain and probably not used anymore - removed as of 06/2012. @zpqrtbnk.
			//
			//   to trigger Umbraco's not-found, one should configure IIS and/or ASP.NET custom 404 errors
			//   so that they point to a non-existing page eg /redirect-404.aspx

			docreq.LookupDomain();
			if (docreq.IsRedirect)
				httpContext.Response.Redirect(docreq.RedirectUrl, true);
			Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = docreq.Culture;
			docreq.LookupDocument();
			if (docreq.IsRedirect)
				httpContext.Response.Redirect(docreq.RedirectUrl, true);

			if (docreq.Is404)
				httpContext.Response.StatusCode = 404;
		}

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
		/// <param name="uri"></param>
		/// <param name="lpath"></param>
		/// <param name="httpContext"></param>
		/// <returns></returns>
		internal bool EnsureUmbracoRoutablePage(Uri uri, string lpath, HttpContextBase httpContext)
		{
			// ensure this is a document request
			if (!EnsureDocumentRequest(httpContext, uri, lpath))
				return false;
			// ensure Umbraco is ready to serve documents
			if (!EnsureIsReady(httpContext, uri))
				return false;
			// ensure Umbraco is properly configured to serve documents
			if (!EnsureIsConfigured(httpContext, uri))
				return false;
			// ensure that its not a base rest handler
			if ((UmbracoSettings.EnableBaseRestHandler) && !EnsureNotBaseRestHandler(lpath))
				return false;

			return true;
		}

		/// <summary>
		/// Entry point for a request
		/// </summary>
		/// <param name="httpContext"></param>
		void ProcessRequest(HttpContextBase httpContext)
		{
			LogHelper.Debug<UmbracoModule>("Start processing request");

			if (IsClientSideRequest(httpContext.Request.Url))
			{
				LogHelper.Debug<UmbracoModule>("End processing request, not transfering to handler, this is a client side file request {0}", () => httpContext.Request.Url);
				return;
			}

			// add Umbraco's signature header
			if (!UmbracoSettings.RemoveUmbracoVersionHeader)
				httpContext.Response.AddHeader("X-Umbraco-Version", string.Format("{0}.{1}", GlobalSettings.VersionMajor, GlobalSettings.VersionMinor));

			//create the legacy UmbracoContext
			global::umbraco.presentation.UmbracoContext.Current = new global::umbraco.presentation.UmbracoContext(httpContext);

			//create the LegacyRequestInitializer
			var legacyRequestInitializer = new LegacyRequestInitializer(httpContext.Request.Url, httpContext);
			// legacy - initialize legacy stuff
			legacyRequestInitializer.InitializeRequest();

			//create the UmbracoContext singleton, one per request!!
			var umbracoContext = new UmbracoContext(
				httpContext,
				ApplicationContext.Current,
				RoutesCacheResolver.Current.RoutesCache);
			UmbracoContext.Current = umbracoContext;

			//create a content store
			var contentStore = new ContentStore(umbracoContext);
			//create the nice urls
			var niceUrls = new NiceUrlProvider(contentStore, umbracoContext);
			//create the RoutingContext
			var routingContext = new RoutingContext(
				DocumentLookupsResolver.Current.DocumentLookups,
				LastChanceLookupResolver.Current.LastChanceLookup,
				contentStore,
				niceUrls);
			//assign the routing context back to the umbraco context
			umbracoContext.RoutingContext = routingContext;

			var uri = httpContext.Request.Url;
			var lpath = uri.AbsolutePath.ToLower();

			// legacy - no idea what this is
			LegacyCleanUmbPageFromQueryString(ref uri, ref lpath);

			//do not continue if this request is not a front-end routable page
			if (EnsureUmbracoRoutablePage(uri, lpath, httpContext))
			{
				//create and assign the document request now that we know its a document being rendered on the front end
				AssignDocumentRequest(httpContext, umbracoContext, uri);

				RewriteToUmbracoHandler(HttpContext.Current, uri.Query, false);
			}
			else
			{
				LogHelper.Debug<UmbracoModule>("End processing request, not transfering to handler {0}", () => uri);
			}


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
				content.Instance.PersistXmlToFile();
			}
		}

		/// <summary>
		/// Ensures that the request is a document request (i.e. one that the module should handle)
		/// </summary>
		/// <param name="httpContext"></param>
		/// <param name="uri"></param>
		/// <param name="lpath"></param>
		/// <returns></returns>
		bool EnsureDocumentRequest(HttpContextBase httpContext, Uri uri, string lpath)
		{
			var maybeDoc = true;

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

				//RewriteToPath<Page>(HttpContext.Current, bootUrl, "", "");
				//TransferRequest(bootUrl, httpContext);

				httpContext.RewritePath(bootUrl);

				return false;
			}

			return true;
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

		// checks if the current request is a /base REST handler request
		// returns false if it is, otherwise true
		bool EnsureNotBaseRestHandler(string lpath)
		{
			// the /base REST handler still lives in umbraco.dll and has
			// not been refactored at the moment. it still is a module,
			// although it should be a handler, or it should be replaced
			// by clean WebAPI.

			// fixme - do it once when initializing the module
			string baseUrl = UriUtility.ToAbsolute(SystemDirectories.Base).ToLower();
			if (!baseUrl.EndsWith("/"))
				baseUrl += "/";
			if (lpath.StartsWith(baseUrl))
			{
				LogHelper.Debug<UmbracoModule>("Detected /base REST handler");

				return false;
			}
			return true;
		}

		/// <summary>
		/// Rewrites to the Umbraco handler.
		/// NOTE: this is a WIP until MVC is implemented properly, currently only webforms is supported.
		/// </summary>		
		/// <param name="context"></param>
		/// <param name="currentQuery"></param>
		/// <param name="isMvc"> </param>
		private static void RewriteToUmbracoHandler(HttpContext context, string currentQuery, bool isMvc)
		{
			var rewritePath = "~/default.aspx" + currentQuery;

			LogHelper.Debug<UmbracoModule>("Transfering to " + rewritePath);

			//NOTE: We do not want to use TransferRequest even though many docs say it is better with IIS7, turns out this is
			//not what we need. The purpose of TransferRequest is to ensure that .net processes all of the rules for the newly
			//rewritten url, but this is not what we want!
			// http://forums.iis.net/t/1146511.aspx

			// Pre MVC 3
			context.RewritePath(rewritePath, false);
			if (isMvc)
			{
				IHttpHandler httpHandler = new MvcHttpHandler();
				httpHandler.ProcessRequest(context);
			}
		}

		#region Legacy

		// "Clean umbPage from querystring, caused by .NET 2.0 default Auth Controls"
		// but really, at the moment I have no idea what this does, and why...
		void LegacyCleanUmbPageFromQueryString(ref Uri uri, ref string lpath)
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
					lpath = path.ToLower();
				}
				//else
				//{
				//    // strip off question mark
				//    query = receivedQuery.Substring(1);
				//}
			}
		}

		#endregion

		#region IHttpModule

		// initialize the module,  this will trigger for each new application
		// and there may be more than 1 application per application domain
		public void Init(HttpApplication app)
		{
			// used to be done in PostAuthorizeRequest but then it disabled OutputCaching due
			// to rewriting happening too early in the chain (Alex Norcliffe 2010-02).
			//app.PostResolveRequestCache += (sender, e) =>

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
		{ }

		#endregion

	}
}
