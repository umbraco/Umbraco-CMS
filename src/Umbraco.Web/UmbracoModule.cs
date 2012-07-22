using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using Umbraco.Core;
using Umbraco.Web.Routing;
using umbraco;
using umbraco.IO;

namespace Umbraco.Web
{
    // also look at IOHelper.ResolveUrlsFromTextString - nightmarish?!

    // context.RewritePath supports ~/ or else must begin with /vdir
    //  Request.RawUrl is still there
    // response.Redirect does?! always remap to /vdir?!

    public class UmbracoModule : IHttpModule
    {
        private static readonly TraceSource Trace = new TraceSource("UmbracoModule");                

        // entry point for a request
        void ProcessRequest(HttpContext httpContext)
        {
            Trace.TraceInformation("Process request");

			//TODO: We need to ensure the below only executes for real requests (i.e. not css, favicon, etc...)
			// I'm pretty sure we need to bind to the PostHandlerAssigned (or whatever event) and follow the same 
			// practices that is in umbraMVCo


            var uri = httpContext.Request.Url;
            var lpath = uri.AbsolutePath.ToLower();

            // add Umbraco's signature header
            if (!UmbracoSettings.RemoveUmbracoVersionHeader)
                httpContext.Response.AddHeader("X-Umbraco-Version", string.Format("{0}.{1}", GlobalSettings.VersionMajor, GlobalSettings.VersionMinor));

			//create the legacy UmbracoContext
			global::umbraco.presentation.UmbracoContext.Current 
				= new global::umbraco.presentation.UmbracoContext(new HttpContextWrapper(httpContext));

            //create the UmbracoContext singleton, one per request!!
            var umbracoContext = new UmbracoContext(
				new HttpContextWrapper(httpContext), 
				ApplicationContext.Current,
				 RoutesCache.Current.GetProvider());
            UmbracoContext.Current = umbracoContext;

			// NO!
			// these are application-wide singletons!

            //create a content store
            var contentStore = new ContentStore(umbracoContext);            
            //create the nice urls
            var niceUrls = new NiceUrlResolver(contentStore, umbracoContext);
            //create the RoutingContext (one per http request)
        	var routingContext = new RoutingContext(
        		umbracoContext,
        		RouteLookups.Current,
        		new ResolveLastChance(),
        		contentStore,
        		niceUrls);
			// NOT HERE BUT SEE **THERE** BELOW
			
			// create the new document request which will cleanup the uri once and for all
            var docreq = new DocumentRequest(uri, routingContext);

			// initialize the DocumentRequest on the UmbracoContext (this is circular dependency but i think in this case is ok)
            umbracoContext.DocumentRequest = docreq;

            //create the LegacyRequestInitializer (one per http request as it relies on the umbraco context!)
            var legacyRequestInitializer = new LegacyRequestInitializer(umbracoContext);

            // legacy - initialize legacy stuff
            legacyRequestInitializer.InitializeRequest();

            // note - at that point the original legacy module did something do handle IIS custom 404 errors
            //   ie pages looking like /anything.aspx?404;/path/to/document - I guess the reason was to support
            //   "directory urls" without having to do wildcard mapping to ASP.NET on old IIS. This is a pain
            //   to maintain and probably not used anymore - removed as of 06/2012. @zpqrtbnk.
            //
            //   to trigger Umbraco's not-found, one should configure IIS and/or ASP.NET custom 404 errors
            //   so that they point to a non-existing page eg /redirect-404.aspx

            var ok = true;

            // ensure this is a document request
            ok = ok && EnsureDocumentRequest(httpContext, uri, lpath);
            // ensure Umbraco is ready to serve documents
            ok = ok && EnsureIsReady(httpContext, uri);
            // ensure Umbraco is properly configured to serve documents
            ok = ok && EnsureIsConfigured(httpContext, uri);
            //TODO: I like the idea of this new setting, but lets get this in to the core at a later time, for now lets just get the basics working.
            //ok = ok && (!Settings.Legacy.EnableBaseRestHandler || EnsureNotBaseRestHandler(httpContext, lpath));
            ok = ok && (EnsureNotBaseRestHandler(httpContext, lpath));

            if (!ok)
            {
                Trace.TraceInformation("End");
                return;
            }

            // legacy - no idea what this is
            LegacyCleanUmbPageFromQueryString(ref uri, ref lpath);

			//**THERE** we should create the doc request
			// before, we're not sure we handling a doc request
			docreq.ResolveDomain();
            if (docreq.IsRedirect)
                httpContext.Response.Redirect(docreq.RedirectUrl, true);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = docreq.Culture;
            docreq.ResolveDocument();
            if (docreq.IsRedirect)
                httpContext.Response.Redirect(docreq.RedirectUrl, true);

            if (docreq.Is404)
                httpContext.Response.StatusCode = 404;

            Trace.TraceInformation("Transfer to UmbracoDefault (default.aspx)");
			TransferRequest("~/default.aspx" + docreq.Uri.Query);

            // it is up to default.aspx to figure out what to display in case
            // there is no document (ugly 404 page?) or no template (blank page?)
        }

        // ensures that the request is a document request
        // ie one that the module should handle
        bool EnsureDocumentRequest(HttpContext httpContext, Uri uri, string lpath)
        {
            bool maybeDoc = true;

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
                Trace.TraceInformation("Not a document");
            return maybeDoc;
        }

        // ensures Umbraco is ready to handle requests
        // if not, set status to 503 and transfer request, and return false
        // if yes, return true
        bool EnsureIsReady(HttpContext httpContext, Uri uri)
        {
            // ensure we are ready
            if (!ApplicationContext.Current.IsReady)
            {
                Trace.TraceEvent(TraceEventType.Warning, 0, "Umbraco is not ready");
                httpContext.Response.StatusCode = 503;
                string bootUrl = null;
                if (UmbracoSettings.EnableSplashWhileLoading) // legacy - should go
                {
					string configPath = UriUtility.ToAbsolute(SystemDirectories.Config);
                    bootUrl = string.Format("{0}/splashes/booting.aspx?url={1}", configPath, HttpUtility.UrlEncode(uri.ToString()));
                    // fixme ?orgurl=... ?retry=...
                }

                //TODO: I like the idea of this new setting, but lets get this in to the core at a later time, for now lets just get the basics working.
                //else if (!string.IsNullOrWhiteSpace(Settings.BootSplashPage))
                //{
                //    bootUrl = UriUtility.ToAbsolute(Settings.BootSplashPage);
                //}

                else
                {
                    // fixme - default.aspx has to be ready for RequestContext.DocumentRequest==null
                    // fixme - in fact we should transfer to an empty html page...
					bootUrl = UriUtility.ToAbsolute("~/default.aspx");
                }
                TransferRequest(bootUrl);
                return false;
            }

            return true;
        }

        // ensures Umbraco is configured
        // if not, redirect to install and return false
        // if yes, return true
        bool EnsureIsConfigured(HttpContext httpContext, Uri uri)
        {
            if (!ApplicationContext.Current.IsConfigured)
            {
                Trace.TraceEvent(TraceEventType.Warning, 0, "Umbraco is not configured");
				string installPath = UriUtility.ToAbsolute(SystemDirectories.Install);
                string installUrl = string.Format("{0}/default.aspx?redir=true&url={1}", installPath, HttpUtility.UrlEncode(uri.ToString()));
                httpContext.Response.Redirect(installUrl, true);
                return false;
            }
            return true;
        }

        // checks if the current request is a /base REST handler request
        // returns false if it is, otherwise true
        bool EnsureNotBaseRestHandler(HttpContext httpContext, string lpath)
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
                Trace.TraceInformation("Detected /base REST handler");
                return false;
            }
            return true;
        }

        // transfers the request using the fastest method available on the server
        void TransferRequest(string path)
        {
            bool integrated = HttpRuntime.UsingIntegratedPipeline;

            // fixme - are we doing this properly?
            // fixme - handle virtual directory?
            // fixme - this does not work 'cos it resets the HttpContext
            //  so we should move the DocumentRequest stuff etc back to default.aspx?
            //  but, also, with TransferRequest, auth & co will run on the new (default.aspx) url,
            //  is that really what we want? I need to talk about it with others. @zpqrtbnk
            integrated = false;

            // http://msmvps.com/blogs/luisabreu/archive/2007/10/09/are-you-using-the-new-transferrequest.aspx
            // http://msdn.microsoft.com/en-us/library/aa344903.aspx
            // http://forums.iis.net/t/1146511.aspx

            if (integrated)
                HttpContext.Current.Server.TransferRequest(path);
            else
                HttpContext.Current.RewritePath(path);
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
			app.PostResolveRequestCache += (sender, e) =>
			{
				HttpContext httpContext = ((HttpApplication)sender).Context;
				ProcessRequest(httpContext);
			};

			// todo: initialize request errors handler
			// todo: initialize XML cache flush
        }

        public void Dispose()
        { }

        #endregion

    }
}
