using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Routing;
using LightInject;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Core.Collections;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.PublishedCache;
using Umbraco.Core.Logging.Serilog.Enrichers;

namespace Umbraco.Web
{
    // also look at IOHelper.ResolveUrlsFromTextString - nightmarish?!

    // context.RewritePath supports ~/ or else must begin with /vdir
    //  Request.RawUrl is still there
    // response.Redirect does?! always remap to /vdir?!

    public class UmbracoModule : IHttpModule
    {
        #region Dependencies

        // modules are *not* instanciated by the container so we have to
        // get our dependencies injected manually, through properties, in
        // Init(). works for dependencies that are singletons.

        [Inject]
        public IUmbracoSettingsSection UmbracoSettings { get; set; }

        [Inject]
        public IGlobalSettings GlobalSettings { get; set; }

        [Inject]
        public IUmbracoContextAccessor UmbracoContextAccessor { get; set; }

        [Inject]
        public IPublishedSnapshotService PublishedSnapshotService { get; set; }

        [Inject]
        public IUserService UserService { get; set; }

        [Inject]
        public UrlProviderCollection UrlProviders { get; set; }

        [Inject]
        public IRuntimeState Runtime { get; set; }

        [Inject]
        public ILogger Logger { get; set; }

        [Inject]
        internal PublishedRouter PublishedRouter { get; set; }

        [Inject]
        internal IUmbracoDatabaseFactory DatabaseFactory { get; set; }

        [Inject]
        internal IVariationContextAccessor VariationContextAccessor { get; set; }

        #endregion

        public UmbracoModule()
        {
            _combinedRouteCollection = new Lazy<RouteCollection>(CreateRouteCollection);
        }

        #region HttpModule event handlers

        /// <summary>
        /// Begins to process a request.
        /// </summary>
        /// <param name="httpContext"></param>
        private void BeginRequest(HttpContextBase httpContext)
        {
            // ensure application url is initialized
            ((RuntimeState) Current.RuntimeState).EnsureApplicationUrl(httpContext.Request);

            // do not process if client-side request
            if (httpContext.Request.Url.IsClientSideRequest())
                return;

            // write the trace output for diagnostics at the end of the request
            httpContext.Trace.Write("UmbracoModule", "Umbraco request begins");

            // ok, process

            // create the UmbracoContext singleton, one per request, and assign
            // replace existing if any (eg during app startup, a temp one is created)
            UmbracoContext.EnsureContext(
                UmbracoContextAccessor,
                httpContext,
                PublishedSnapshotService,
                new WebSecurity(httpContext, UserService, GlobalSettings),
                UmbracoConfig.For.UmbracoSettings(),
                UrlProviders,
                GlobalSettings,
                VariationContextAccessor,
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

            var umbracoContext = UmbracoContext.Current;

            // re-write for the default back office path
            if (httpContext.Request.Url.IsDefaultBackOfficeRequest(GlobalSettings))
            {
                if (EnsureRuntime(httpContext, umbracoContext.OriginalRequestUrl))
                    RewriteToBackOfficeHandler(httpContext);
                return;
            }

            // do not process if this request is not a front-end routable page
            var isRoutableAttempt = EnsureUmbracoRoutablePage(umbracoContext, httpContext);

            // raise event here
            OnRouteAttempt(new RoutableAttemptEventArgs(isRoutableAttempt.Result, umbracoContext, httpContext));
            if (isRoutableAttempt.Success == false) return;

            httpContext.Trace.Write("UmbracoModule", "Umbraco request confirmed");

            // ok, process

            // note: requestModule.UmbracoRewrite also did some stripping of &umbPage
            // from the querystring... that was in v3.x to fix some issues with pre-forms
            // auth. Paul Sterling confirmed in jan. 2013 that we can get rid of it.

            // instanciate, prepare and process the published content request
            // important to use CleanedUmbracoUrl - lowercase path-only version of the current url
            var request = PublishedRouter.CreateRequest(umbracoContext);
            umbracoContext.PublishedRequest = request;
            PublishedRouter.PrepareRequest(request);

            // HandleHttpResponseStatus returns a value indicating that the request should
            // not be processed any further, eg because it has been redirect. then, exit.
            if (HandleHttpResponseStatus(httpContext, request, Logger))
                return;

            if (request.HasPublishedContent == false)
                httpContext.RemapHandler(new PublishedContentNotFoundHandler());
            else
                RewriteToUmbracoHandler(httpContext, request);
        }

        #endregion

        #region Methods

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
            if (EnsureDocumentRequest(httpContext, uri) == false)
            {
                reason = EnsureRoutableOutcome.NotDocumentRequest;
            }
            // ensure the runtime is in the proper state
            // and deal with needed redirects, etc
            else if (EnsureRuntime(httpContext, uri) == false)
            {
                reason = EnsureRoutableOutcome.NotReady;
            }
            // ensure Umbraco has documents to serve
            else if (EnsureHasContent(context, httpContext) == false)
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
        private bool EnsureDocumentRequest(HttpContextBase httpContext, Uri uri)
        {
            var maybeDoc = true;
            var lpath = uri.AbsolutePath.ToLowerInvariant();

            // handle directory-urls used for asmx
            // legacy - what's the point really?
            if (/*maybeDoc &&*/ GlobalSettings.UseDirectoryUrls)
            {
                var asmxPos = lpath.IndexOf(".asmx/", StringComparison.OrdinalIgnoreCase);
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
            var extension = Path.GetExtension(lpath);
            if (maybeDoc && extension.IsNullOrWhiteSpace() == false && extension != ".aspx")
                maybeDoc = false;

            // at that point, either we have no extension, or it is .aspx

            // if the path is reserved then it cannot be a document request
            if (maybeDoc && GlobalSettings.IsReservedPathOrUrl(lpath, httpContext, _combinedRouteCollection.Value))
                maybeDoc = false;

            //NOTE: No need to warn, plus if we do we should log the document, as this message doesn't really tell us anything :)
            //if (!maybeDoc)
            //{
            //    Logger.Warn<UmbracoModule>("Not a document");
            //}

            return maybeDoc;
        }

        private bool EnsureRuntime(HttpContextBase httpContext, Uri uri)
        {
            var debug = Runtime.Debug;
            var level = Runtime.Level;
            switch (level)
            {
                case RuntimeLevel.Unknown:
                case RuntimeLevel.Boot:
                    // not ready yet, but wait
                    ReportRuntime(level, "Umbraco is booting.");

                    // let requests pile up and wait for 10s then show the splash anyway
                    if (UmbracoConfig.For.UmbracoSettings().Content.EnableSplashWhileLoading == false
                        && ((RuntimeState) Runtime).WaitForRunLevel(TimeSpan.FromSeconds(10))) return true;

                    // redirect to booting page
                    httpContext.Response.StatusCode = 503; // temp not available
                    const string bootUrl = "~/config/splashes/booting.aspx";
                    httpContext.Response.AddHeader("Retry-After", debug ? "1" : "30"); // seconds
                    httpContext.RewritePath(UriUtility.ToAbsolute(bootUrl) + "?url=" + HttpUtility.UrlEncode(uri.ToString()));
                    return false; // cannot serve content

                case RuntimeLevel.BootFailed:
                    // redirect to death page
                    ReportRuntime(level, "Umbraco has failed.");

                    httpContext.Response.StatusCode = 503; // temp not available
                    const string deathUrl = "~/config/splashes/death.aspx";
                    httpContext.Response.AddHeader("Retry-After", debug ? "1" : "300"); // seconds
                    httpContext.RewritePath(UriUtility.ToAbsolute(deathUrl) + "?url=" + HttpUtility.UrlEncode(uri.ToString()));
                    return false; // cannot serve content

                case RuntimeLevel.Run:
                    // ok
                    return true;

                case RuntimeLevel.Install:
                case RuntimeLevel.Upgrade:
                    // redirect to install
                    ReportRuntime(level, "Umbraco must install or upgrade.");
                    var installPath = UriUtility.ToAbsolute(SystemDirectories.Install);
                    var installUrl = $"{installPath}/?redir=true&url={HttpUtility.UrlEncode(uri.ToString())}";
                    httpContext.Response.Redirect(installUrl, true);
                    return false; // cannot serve content

                default:
                    throw new NotSupportedException($"Unexpected runtime level: {Current.RuntimeState.Level}.");
            }
        }

        private static bool _reported;
        private static RuntimeLevel _reportedLevel;

        private void ReportRuntime(RuntimeLevel level, string message)
        {
            if (_reported && _reportedLevel == level) return;
            _reported = true;
            _reportedLevel = level;
            Logger.Warn<UmbracoModule>(message);
        }

        // ensures Umbraco has at least one published node
        // if not, rewrites to splash and return false
        // if yes, return true
        private bool EnsureHasContent(UmbracoContext context, HttpContextBase httpContext)
        {
            if (context.ContentCache.HasContent())
                return true;

            Logger.Warn<UmbracoModule>("Umbraco has no content");

            const string noContentUrl = "~/config/splashes/noNodes.aspx";
            httpContext.RewritePath(UriUtility.ToAbsolute(noContentUrl));

            return false;
        }

        // returns a value indicating whether redirection took place and the request has
        // been completed - because we don't want to Response.End() here to terminate
        // everything properly.
        internal static bool HandleHttpResponseStatus(HttpContextBase context, PublishedRequest pcr, ILogger logger)
        {
            var end = false;
            var response = context.Response;

            logger.Debug<UmbracoModule>("Response status: Redirect={Redirect}, Is404={Is404}, StatusCode={ResponseStatusCode}",
                pcr.IsRedirect ? (pcr.IsRedirectPermanent ? "permanent" : "redirect") : "none",
                pcr.Is404 ? "true" : "false",
                pcr.ResponseStatusCode);

            if(pcr.Cacheability != default)
                response.Cache.SetCacheability(pcr.Cacheability);

            foreach (var cacheExtension in pcr.CacheExtensions)
                response.Cache.AppendCacheExtension(cacheExtension);

            foreach (var header in pcr.Headers)
                response.AppendHeader(header.Key, header.Value);

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
                    logger.Warn<UmbracoModule>("Status code is 404 yet TrySkipIisCustomErrors is false - IIS will take over.");
            }

            if (pcr.ResponseStatusCode > 0)
            {
                // set status code -- even for redirects
                response.StatusCode = pcr.ResponseStatusCode;
                response.StatusDescription = pcr.ResponseStatusDescription;
            }
            //if (pcr.IsRedirect)
            //    response.End(); // end response -- kills the thread and does not return!

            if (pcr.IsRedirect == false) return end;

            response.Flush();
            // bypass everything and directly execute EndRequest event -- but returns
            context.ApplicationInstance.CompleteRequest();
            // though some say that .CompleteRequest() does not properly shutdown the response
            // and the request will hang until the whole code has run... would need to test?
            logger.Debug<UmbracoModule>("Response status: redirecting, complete request now.");

            return end;
        }

        /// <summary>
        /// Rewrites to the default back office page.
        /// </summary>
        /// <param name="context"></param>
        private void RewriteToBackOfficeHandler(HttpContextBase context)
        {
            // GlobalSettings.Path has already been through IOHelper.ResolveUrl() so it begins with / and vdir (if any)
            var rewritePath = GlobalSettings.Path.TrimEnd('/') + "/Default";
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
        private void RewriteToUmbracoHandler(HttpContextBase context, PublishedRequest pcr)
        {
            // NOTE: we do not want to use TransferRequest even though many docs say it is better with IIS7, turns out this is
            // not what we need. The purpose of TransferRequest is to ensure that .net processes all of the rules for the newly
            // rewritten url, but this is not what we want!
            // read: http://forums.iis.net/t/1146511.aspx

            var query = pcr.Uri.Query.TrimStart('?');

            // GlobalSettings.Path has already been through IOHelper.ResolveUrl() so it begins with / and vdir (if any)
            var rewritePath = GlobalSettings.Path.TrimEnd('/') + "/RenderMvc";
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
        private void DisposeHttpContextItems(HttpContext http)
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
                    Logger.Error<UmbracoModule>(ex, "Could not dispose item with key {Key}", k);
                }
                try
                {
                    k.DisposeIfDisposable();
                }
                catch (Exception ex)
                {
                    Logger.Error<UmbracoModule>(ex, "Could not dispose item key {Key}", k);
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
            if (Core.Composing.Current.RuntimeState.Level == RuntimeLevel.BootFailed)
            {
                // there's nothing we can do really
                app.BeginRequest += (sender, args) =>
                {
                    // would love to avoid throwing, and instead display a customized Umbraco 500
                    // page - however if we don't throw here, something else might go wrong, and
                    // it's this later exception that would be reported. could not figure out how
                    // to prevent it, either with httpContext.Response.End() or .ApplicationInstance
                    // .CompleteRequest()

                    // also, if something goes wrong with our DI setup, the logging subsystem may
                    // not even kick in, so here we try to give as much detail as possible

                    Exception e = Core.Composing.Current.RuntimeState.BootFailedException;
                    if (e == null)
                        throw new BootFailedException(BootFailedException.DefaultMessage);
                    var m = new StringBuilder();
                    m.Append(BootFailedException.DefaultMessage);
                    while (e != null)
                    {
                        m.Append($"\n\n-> {e.GetType().FullName}: {e.Message}");
                        if (string.IsNullOrWhiteSpace(e.StackTrace) == false)
                            m.Append($"\n{e.StackTrace}");
                        e = e.InnerException;
                    }
                    throw new BootFailedException(m.ToString());
                };
                return;
            }

            // modules are *not* instanciated by the container so we have to
            // get our dependencies injected manually, through properties.
            Core.Composing.Current.Container.InjectProperties(this);

            //Create a GUID to use as some form of request ID
            var requestId = Guid.Empty;

            app.BeginRequest += (sender, e) =>
            {
                var httpContext = ((HttpApplication) sender).Context;

                var httpRequestId = Guid.Empty;
                LogHttpRequest.TryGetCurrentHttpRequestId(out httpRequestId);

                Logger.Verbose<UmbracoModule>("Begin request [{HttpRequestId}]: {RequestUrl}",
                    httpRequestId,
                    httpContext.Request.Url);

                BeginRequest(new HttpContextWrapper(httpContext));
            };

            //disable asp.net headers (security)
            // This is the correct place to modify headers according to MS:
            // https://our.umbraco.com/forum/umbraco-7/using-umbraco-7/65241-Heap-error-from-header-manipulation?p=0#comment220889
            app.PostReleaseRequestState += (sender, args) =>
            {
                var httpContext = ((HttpApplication) sender).Context;
                try
                {
                    httpContext.Response.Headers.Remove("Server");
                    //this doesn't normally work since IIS sets it but we'll keep it here anyways.
                    httpContext.Response.Headers.Remove("X-Powered-By");
                    httpContext.Response.Headers.Remove("X-AspNet-Version");
                    httpContext.Response.Headers.Remove("X-AspNetMvc-Version");
                }
                catch (PlatformNotSupportedException)
                {
                    // can't remove headers this way on IIS6 or cassini.
                }
            };

            app.PostAuthenticateRequest += (sender, e) =>
            {
                var httpContext = ((HttpApplication) sender).Context;
                //ensure the thread culture is set
                httpContext.User?.Identity?.EnsureCulture();
            };

            app.PostResolveRequestCache += (sender, e) =>
            {
                var httpContext = ((HttpApplication) sender).Context;
                ProcessRequest(new HttpContextWrapper(httpContext));
            };

            app.EndRequest += (sender, args) =>
            {
                var httpContext = ((HttpApplication) sender).Context;

                if (UmbracoContext.Current != null)
                {
                    var httpRequestId = Guid.Empty;
                    LogHttpRequest.TryGetCurrentHttpRequestId(out httpRequestId);

                    Logger.Verbose<UmbracoModule>(
                        "End request [{HttpRequestId}]: {RequestUrl} took {Duration}ms",
                        httpRequestId,
                        httpContext.Request.Url,
                        DateTime.Now.Subtract(UmbracoContext.Current.ObjectCreated).TotalMilliseconds);
                }

                OnEndRequest(new UmbracoRequestEventArgs(UmbracoContext.Current, new HttpContextWrapper(httpContext)));

                DisposeHttpContextItems(httpContext);
            };
        }

        public void Dispose()
        { }

        #endregion

        #region Events

        public static event EventHandler<RoutableAttemptEventArgs> RouteAttempt;

        private void OnRouteAttempt(RoutableAttemptEventArgs args)
        {
            RouteAttempt?.Invoke(this, args);
        }

        public static event EventHandler<UmbracoRequestEventArgs> EndRequest;

        private void OnEndRequest(UmbracoRequestEventArgs args)
        {
            EndRequest?.Invoke(this, args);
        }

        #endregion

        /// <summary>
        /// This is used to be passed into the GlobalSettings.IsReservedPathOrUrl and will include some 'fake' routes
        /// used to determine if a path is reserved.
        /// </summary>
        /// <remarks>
        /// This is basically used to reserve paths dynamically
        /// </remarks>
        private readonly Lazy<RouteCollection> _combinedRouteCollection;

        private RouteCollection CreateRouteCollection()
        {
            var routes = new RouteCollection();

            foreach (var route in RouteTable.Routes)
                routes.Add(route);

            foreach (var reservedPath in ReservedPaths)
            {
                try
                {
                    routes.Add("_umbreserved_" + reservedPath.ReplaceNonAlphanumericChars(""),
                        new Route(reservedPath.TrimStart('/'), new StopRoutingHandler()));
                }
                catch (Exception ex)
                {
                    Logger.Error<UmbracoModule>(ex, "Could not add reserved path route");
                }
            }

            return routes;
        }

        /// <summary>
        /// This is used internally to track any registered callback paths for Identity providers. If the request path matches
        /// any of the registered paths, then the module will let the request keep executing
        /// </summary>
        internal static readonly ConcurrentHashSet<string> ReservedPaths = new ConcurrentHashSet<string>();
    }
}
