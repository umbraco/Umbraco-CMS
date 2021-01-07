using System;
using System.Web;
using System.Web.Routing;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Hosting;
using Umbraco.Core.Security;
using Umbraco.Web.Composing;
using Umbraco.Web.Routing;
using RouteDirection = Umbraco.Web.Routing.RouteDirection;

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
        private readonly IRuntimeState _runtime;
        private readonly ILogger _logger;
        private readonly IPublishedRouter _publishedRouter;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly RoutableDocumentFilter _routableDocumentLookup;
        private readonly IRequestCache _requestCache;
        private readonly GlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly UriUtility _uriUtility;

        public UmbracoInjectedModule(
            IRuntimeState runtime,
            ILogger logger,
            IPublishedRouter publishedRouter,
            IUmbracoContextFactory umbracoContextFactory,
            RoutableDocumentFilter routableDocumentLookup,
            UriUtility uriUtility,
            IRequestCache requestCache,
            GlobalSettings globalSettings,
            IHostingEnvironment hostingEnvironment)
        {
            _runtime = runtime;
            _logger = logger;
            _publishedRouter = publishedRouter;
            _umbracoContextFactory = umbracoContextFactory;
            _routableDocumentLookup = routableDocumentLookup;
            _uriUtility = uriUtility;
            _requestCache = requestCache;
            _globalSettings = globalSettings;
            _hostingEnvironment = hostingEnvironment;
        }

        #region HttpModule event handlers

        /// <summary>
        /// Begins to process a request.
        /// </summary>
        /// <param name="httpContext"></param>
        private void BeginRequest(HttpContextBase httpContext)
        {
            // do not process if client-side request
            if (httpContext.Request.Url.IsClientSideRequest())
                return;

            // write the trace output for diagnostics at the end of the request
            httpContext.Trace.Write("UmbracoModule", "Umbraco request begins");

            // ok, process

            // TODO: should we move this to after we've ensured we are processing a routable page?
            // ensure there's an UmbracoContext registered for the current request
            // registers the context reference so its disposed at end of request
            var umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();
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
            if (httpContext.Request.Url.IsDefaultBackOfficeRequest(_globalSettings, _hostingEnvironment))
            {
                if (EnsureRuntime(httpContext, umbracoContext.OriginalRequestUrl))
                    RewriteToBackOfficeHandler(httpContext);
                return;
            }

            // do not process if this request is not a front-end routable page
            var isRoutableAttempt = EnsureUmbracoRoutablePage(umbracoContext, httpContext);

            // raise event here
            UmbracoModule.OnRouteAttempt(this, new RoutableAttemptEventArgs(isRoutableAttempt.Result, umbracoContext));
            if (isRoutableAttempt.Success == false) return;

            httpContext.Trace.Write("UmbracoModule", "Umbraco request confirmed");

            // ok, process

            // note: requestModule.UmbracoRewrite also did some stripping of &umbPage
            // from the querystring... that was in v3.x to fix some issues with pre-forms
            // auth. Paul Sterling confirmed in Jan. 2013 that we can get rid of it.

            // instantiate, prepare and process the published content request
            // important to use CleanedUmbracoUrl - lowercase path-only version of the current URL
            var requestBuilder = _publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl).Result;
            var request = umbracoContext.PublishedRequest = _publishedRouter.RouteRequestAsync(requestBuilder, new RouteRequestOptions(RouteDirection.Inbound)).Result;

            // NOTE: This has been ported to netcore
            // HandleHttpResponseStatus returns a value indicating that the request should
            // not be processed any further, eg because it has been redirect. then, exit.
            //if (UmbracoModule.HandleHttpResponseStatus(httpContext, request, _logger))
            //    return;
            //if (request.HasPublishedContent() == false)
            //    httpContext.RemapHandler(new PublishedContentNotFoundHandler());
            //else
            //    RewriteToUmbracoHandler(httpContext, request);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks the current request and ensures that it is routable based on the structure of the request and URI
        /// </summary>
        internal Attempt<EnsureRoutableOutcome> EnsureUmbracoRoutablePage(IUmbracoContext context, HttpContextBase httpContext)
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

        // TODO: Where should this execute in netcore? This will have to be a middleware
        // executing before UseRouting so that it is done before any endpoint routing takes place.
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

                    // NOTE: We have moved the logic that was here to netcore already

                    return false; // cannot serve content

                default:
                    throw new NotSupportedException($"Unexpected runtime level: {level}.");
            }
        }

        // ensures Umbraco has at least one published node
        // if not, rewrites to splash and return false
        // if yes, return true
        private bool EnsureHasContent(IUmbracoContext context, HttpContextBase httpContext)
        {
            if (context.Content.HasContent())
                return true;

            _logger.LogWarning("Umbraco has no content");

            if (RouteTable.Routes[Constants.Web.NoContentRouteName] is Route route)
            {
                httpContext.RewritePath(route.Url);
            }

            return false;
        }

        /// <summary>
        /// Rewrites to the default back office page.
        /// </summary>
        /// <param name="context"></param>
        private void RewriteToBackOfficeHandler(HttpContextBase context)
        {
            // GlobalSettings.Path has already been through IOHelper.ResolveUrl() so it begins with / and vdir (if any)
            var rewritePath = _globalSettings.GetBackOfficePath(_hostingEnvironment).TrimEnd('/') + "/Default";
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
                var httpContext = ((HttpApplication) sender).Context;

                BeginRequest(new HttpContextWrapper(httpContext));
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

                UmbracoModule.OnEndRequest(this, new UmbracoRequestEventArgs(Current.UmbracoContext));
            };
        }

        public void Dispose()
        { }

        #endregion


    }
}
