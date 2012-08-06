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
		/// <param name="docLookups"> </param>
		/// <param name="lastChanceLookup"> </param>
		internal bool ProcessFrontEndDocumentRequest(
			HttpContextBase httpContext, 
			UmbracoContext umbracoContext,
			IEnumerable<IDocumentLookup> docLookups,
			IDocumentLastChanceLookup lastChanceLookup)
		{
			if (httpContext == null) throw new ArgumentNullException("httpContext");
			if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");

			if (httpContext.Request.Url.LocalPath.InvariantStartsWith("/default.aspx")
				&& !string.IsNullOrWhiteSpace(httpContext.Request.QueryString["path"]))
			{
				//the path is the original path that the request came in on before we've rewritten it,
				//this is required because TransferRequest does not maintain the httpcontext
				var path = httpContext.Request.QueryString["path"];
				var qry = httpContext.Request.QueryString["qry"];
				Uri uri;
				try
				{
					uri = UriUtility.ToFullUrl(path + qry, httpContext);
				}
				catch (Exception ex)
				{
					//if this fails then the path could not be parsed to a Uri which could be something malicious
					LogHelper.Error<UmbracoModule>(string.Format("Could not parse the path {0} into a full Uri", path), ex);
					return false;
				}

				//create request based objects (one per http request)...

				//create a content store
				var contentStore = new ContentStore(umbracoContext);
				//create the nice urls
				var niceUrls = new NiceUrlProvider(contentStore, umbracoContext);
				//create the RoutingContext
				var routingContext = new RoutingContext(
					umbracoContext,
					docLookups,
					lastChanceLookup,
					contentStore,
					niceUrls);
				// create the new document request which will cleanup the uri once and for all
				var docreq = new DocumentRequest(uri, routingContext);
				// initialize the DocumentRequest on the UmbracoContext (this is circular dependency but i think in this case is ok)
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

				// it is up to default.aspx to figure out what to display in case
				// there is no document (ugly 404 page?) or no template (blank page?)
				return true;
			}

			return false;
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

			//Does a check to see if this current request contains the information in order to process the
			//request as a front-end request. If not, then its because the rewrite hasn't taken place yet.
			//if we need to rewrite, then we'll cleanup the query strings and rewrite to the front end handler.
			if (!ProcessFrontEndDocumentRequest(httpContext, umbracoContext, 
				DocumentLookupsResolver.Current.DocumentLookups,
				LastChanceLookupResolver.Current.LastChanceLookup))
			{
				var uri = httpContext.Request.Url;
				var lpath = uri.AbsolutePath.ToLower();

				// legacy - no idea what this is
				LegacyCleanUmbPageFromQueryString(ref uri, ref lpath);

				//do not continue if this request is not a front-end routable page
				if (EnsureUmbracoRoutablePage(uri, lpath, httpContext))
				{
					RewriteToPath<Page>(HttpContext.Current, lpath, uri.Query);
					//RewriteToPath<Page>(HttpContext.Current, "", docreq.Uri.Query);
				}
				else
				{
					LogHelper.Debug<UmbracoModule>("End processing request, not transfering to handler {0}", () => uri);
				}
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

		private static void RewriteToPath<THandler>(HttpContext context, string currentPath, string currentQuery)
			where THandler : IHttpHandler
		{

			if ((context.CurrentHandler is THandler)) return;

			var rewritePath = "~/default.aspx?path="
							  + context.Server.UrlEncode(currentPath)
							  + "&qry="
							  + context.Server.UrlEncode(currentQuery);

			if (currentPath.StartsWith(rewritePath, StringComparison.InvariantCultureIgnoreCase)) return;

			var isMvc = TypeHelper.IsTypeAssignableFrom<THandler>(typeof(MvcHandler));

			LogHelper.Debug<UmbracoModule>("Transfering to " + rewritePath);

			if (HttpRuntime.UsingIntegratedPipeline)
			{
				context.Server.TransferRequest(rewritePath, true);
			}
			else
			{
				// Pre MVC 3
				context.RewritePath(rewritePath, false);
				if (isMvc)
				{
					IHttpHandler httpHandler = new MvcHttpHandler();
					httpHandler.ProcessRequest(context);
				}
			}
		}

		//// transfers the request using the fastest method available on the server
		//void TransferRequest(string path, HttpContextBase httpContext)
		//{
		//    LogHelper.Debug<UmbracoModule>("Transfering to " + path);

		//    var integrated = HttpRuntime.UsingIntegratedPipeline;

		//    // fixme - are we doing this properly?
		//    // fixme - handle virtual directory?
		//    // fixme - this does not work 'cos it resets the HttpContext
		//    //  so we should move the DocumentRequest stuff etc back to default.aspx?
		//    //  but, also, with TransferRequest, auth & co will run on the new (default.aspx) url,
		//    //  is that really what we want? I need to talk about it with others. @zpqrtbnk

		//    // NOTE: SD: Need to look at how umbraMVCo does this. It is true that the TransferRequest initializes a new HttpContext,
		//    //  what we need to do is when we transfer to the handler we send a query string with the found page Id to be looked up
		//    //  after we have done our routing check. This however needs some though as we don't want to have to query for this
		//    //  page twice. Again, I'll check how umbraMVCo is doing it as I had though about that when i created it :)

		//    integrated = false;

		//    // http://msmvps.com/blogs/luisabreu/archive/2007/10/09/are-you-using-the-new-transferrequest.aspx
		//    // http://msdn.microsoft.com/en-us/library/aa344903.aspx
		//    // http://forums.iis.net/t/1146511.aspx

		//    if (integrated)
		//        httpContext.Server.TransferRequest(path);
		//    else
		//        httpContext.RewritePath(path);
		//}



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

			//SD: changed to post map request handler so we can know what the handler actually is, this is a better fit for
			//when we handle the routing
			app.PostMapRequestHandler += (sender, e) =>
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
