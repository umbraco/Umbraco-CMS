using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Web.Routing;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Security;
using Umbraco.Web.Composing;

namespace Umbraco.Web
{
    // notes
    //
    // also look at IOHelper.ResolveUrlsFromTextString - nightmarish?!
    //
    // context.RewritePath supports ~/ or else must begin with /vdir
    //  Request.RawUrl is still there
    // response.Redirect does?! always remap to /vdir?!

    /// <summary>
    /// Represents the main Umbraco module.
    /// </summary>
    /// <remarks>
    /// <para>Is registered by the <see cref="Umbraco.Web.Runtime.WebRuntime"/>.</para>
    /// <para>Do *not* try to use that one as a module in web.config.</para>
    /// </remarks>
    public class UmbracoInjectedModule : IHttpModule
    {
        private readonly IGlobalSettings _globalSettings;
        private readonly IRuntimeState _runtime;
        private readonly ILogger _logger;
        private readonly IPublishedRouter _publishedRouter;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly RoutableDocumentFilter _routableDocumentLookup;

        public UmbracoInjectedModule(
            IGlobalSettings globalSettings,
            IRuntimeState runtime,
            ILogger logger,
            IPublishedRouter publishedRouter,
            IUmbracoContextFactory umbracoContextFactory,
            RoutableDocumentFilter routableDocumentLookup)
        {
            _globalSettings = globalSettings;
            _runtime = runtime;
            _logger = logger;
            _publishedRouter = publishedRouter;
            _umbracoContextFactory = umbracoContextFactory;
            _routableDocumentLookup = routableDocumentLookup;
        }

        #region HttpModule event handlers

        /// <summary>
        /// Begins to process a request.
        /// </summary>
        /// <param name="httpContext"></param>
        private void BeginRequest(HttpContextBase httpContext)
        {
            // ensure application URL is initialized
            ((RuntimeState) Current.RuntimeState).EnsureApplicationUrl(httpContext.Request);

            // do not process if client-side request
            if (httpContext.Request.Url.IsClientSideRequest())
                return;

            // write the trace output for diagnostics at the end of the request
            httpContext.Trace.Write("UmbracoModule", "Umbraco request begins");

            // ok, process

            // TODO: should we move this to after we've ensured we are processing a routable page?
            // ensure there's an UmbracoContext registered for the current request
            // registers the context reference so its disposed at end of request
            var umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext(httpContext);
            httpContext.DisposeOnPipelineCompleted(umbracoContextReference);
        }

        /// <summary>
        /// Processes the Umbraco Request
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

            if (Current.UmbracoContext == null)
                throw new InvalidOperationException("The Current.UmbracoContext is null, ProcessRequest cannot proceed unless there is a current UmbracoContext");

            var umbracoContext = Current.UmbracoContext;

            // re-write for the default back office path
            if (httpContext.Request.Url.IsDefaultBackOfficeRequest(_globalSettings))
            {
                if (EnsureRuntime(httpContext, umbracoContext.OriginalRequestUrl))
                    RewriteToBackOfficeHandler(httpContext);
                return;
            }

            // do not process if this request is not a front-end routable page
            var isRoutableAttempt = EnsureUmbracoRoutablePage(umbracoContext, httpContext);

            // raise event here
            UmbracoModule.OnRouteAttempt(this, new RoutableAttemptEventArgs(isRoutableAttempt.Result, umbracoContext, httpContext));
            if (isRoutableAttempt.Success == false) return;

            httpContext.Trace.Write("UmbracoModule", "Umbraco request confirmed");

            // ok, process

            // note: requestModule.UmbracoRewrite also did some stripping of &umbPage
            // from the querystring... that was in v3.x to fix some issues with pre-forms
            // auth. Paul Sterling confirmed in Jan. 2013 that we can get rid of it.

            // instantiate, prepare and process the published content request
            // important to use CleanedUmbracoUrl - lowercase path-only version of the current URL
            var request = _publishedRouter.CreateRequest(umbracoContext);
            umbracoContext.PublishedRequest = request;
            _publishedRouter.PrepareRequest(request);

            // HandleHttpResponseStatus returns a value indicating that the request should
            // not be processed any further, eg because it has been redirect. then, exit.
            if (UmbracoModule.HandleHttpResponseStatus(httpContext, request, _logger))
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
            if (!_routableDocumentLookup.IsDocumentRequest(httpContext, context.OriginalRequestUrl))
            {
                reason = EnsureRoutableOutcome.NotDocumentRequest;
            }
            // ensure the runtime is in the proper state
            // and deal with needed redirects, etc
            else if (!EnsureRuntime(httpContext, uri))
            {
                reason = EnsureRoutableOutcome.NotReady;
            }
            // ensure Umbraco has documents to serve
            else if (!EnsureHasContent(context, httpContext))
            {
                reason = EnsureRoutableOutcome.NoContent;
            }

            return Attempt.If(reason == EnsureRoutableOutcome.IsRoutable, reason);
        }

        

        private bool EnsureRuntime(HttpContextBase httpContext, Uri uri)
        {
            var level = _runtime.Level;
            switch (level)
            {
                // we should never handle Unknown nor Boot: the runtime boots in Application_Start
                // and as long as it has not booted, no request other than the initial request is
                // going to be served (see https://stackoverflow.com/a/21402100)
                // we should never handle BootFailed: if boot failed, the pipeline should not run
                // at all
                case RuntimeLevel.Unknown:
                case RuntimeLevel.Boot:
                case RuntimeLevel.BootFailed:
                    throw new PanicException($"Unexpected runtime level: {level}.");

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
                    throw new NotSupportedException($"Unexpected runtime level: {level}.");
            }
        }

        private static bool _reported;
        private static RuntimeLevel _reportedLevel;

        private void ReportRuntime(RuntimeLevel level, string message)
        {
            if (_reported && _reportedLevel == level) return;
            _reported = true;
            _reportedLevel = level;
            _logger.Warn<UmbracoModule>(message);
        }

        // ensures Umbraco has at least one published node
        // if not, rewrites to splash and return false
        // if yes, return true
        private bool EnsureHasContent(UmbracoContext context, HttpContextBase httpContext)
        {
            if (context.Content.HasContent())
                return true;

            _logger.Warn<UmbracoModule>("Umbraco has no content");

            const string noContentUrl = "~/config/splashes/noNodes.aspx";
            httpContext.RewritePath(UriUtility.ToAbsolute(noContentUrl));

            return false;
        }

        /// <summary>
        /// Rewrites to the default back office page.
        /// </summary>
        /// <param name="context"></param>
        private void RewriteToBackOfficeHandler(HttpContextBase context)
        {
            // GlobalSettings.Path has already been through IOHelper.ResolveUrl() so it begins with / and vdir (if any)
            var rewritePath = _globalSettings.Path.TrimEnd(Constants.CharArrays.ForwardSlash) + "/Default";
            // rewrite the path to the path of the handler (i.e. /umbraco/RenderMvc)
            context.RewritePath(rewritePath, "", "", false);

            //if it is MVC we need to do something special, we are not using TransferRequest as this will
            //require us to rewrite the path with query strings and then re-parse the query strings, this would
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
            // rewritten URL, but this is not what we want!
            // read: http://forums.iis.net/t/1146511.aspx

            var query = pcr.Uri.Query.TrimStart(Constants.CharArrays.QuestionMark);

            // GlobalSettings.Path has already been through IOHelper.ResolveUrl() so it begins with / and vdir (if any)
            var rewritePath = _globalSettings.Path.TrimEnd(Constants.CharArrays.ForwardSlash) + "/RenderMvc";
            // rewrite the path to the path of the handler (i.e. /umbraco/RenderMvc)
            context.RewritePath(rewritePath, "", query, false);

            //if it is MVC we need to do something special, we are not using TransferRequest as this will
            //require us to rewrite the path with query strings and then re-parse the query strings, this would
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
                    _logger.Error<UmbracoModule>(ex, "Could not dispose item with key {Key}", k);
                }
                try
                {
                    k.DisposeIfDisposable();
                }
                catch (Exception ex)
                {
                    _logger.Error<UmbracoModule>(ex, "Could not dispose item key {Key}", k);
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
            if (_runtime.Level == RuntimeLevel.BootFailed)
            {
                // there's nothing we can do really
                app.BeginRequest += (sender, args) =>
                {
                    // if we don't throw here, something else might go wrong,
                    // and it's this later exception that would be reported.

                    // also, if something goes wrong with our DI setup, the logging subsystem may
                    // not even kick in, so here we try to give as much detail as possible

                    // the exception is handled in UmbracoApplication which shows a custom error page
                    BootFailedException.Rethrow(Core.Composing.Current.RuntimeState.BootFailedException);
                };
                return;
            }

            app.BeginRequest += (sender, e) =>
            {
                var httpContext = ((HttpApplication) sender).Context;

                LogHttpRequest.TryGetCurrentHttpRequestId(out var httpRequestId);

                _logger.Verbose<UmbracoModule>("Begin request [{HttpRequestId}]: {RequestUrl}", httpRequestId, httpContext.Request.Url);
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

                if (Current.UmbracoContext != null && Current.UmbracoContext.IsFrontEndUmbracoRequest)
                {
                    LogHttpRequest.TryGetCurrentHttpRequestId(out var httpRequestId);

                    _logger.Verbose<UmbracoModule>("End Request [{HttpRequestId}]: {RequestUrl} ({RequestDuration}ms)", httpRequestId, httpContext.Request.Url, DateTime.Now.Subtract(Current.UmbracoContext.ObjectCreated).TotalMilliseconds);
                }

                UmbracoModule.OnEndRequest(this, new UmbracoRequestEventArgs(Current.UmbracoContext, new HttpContextWrapper(httpContext)));

                DisposeHttpContextItems(httpContext);
            };
        }

        public void Dispose()
        { }

        #endregion

        
    }
}
