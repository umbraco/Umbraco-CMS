using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Smidge.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Infrastructure.WebAssets;
using Umbraco.Cms.Web.Common.Profiler;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Middleware
{

    /// <summary>
    /// Manages Umbraco request objects and their lifetime
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is responsible for initializing the content cache
    /// </para>
    /// <para>
    /// This is responsible for creating and assigning an <see cref="IUmbracoContext"/>
    /// </para>
    /// </remarks>
    public class UmbracoRequestMiddleware : IMiddleware
    {
        private readonly ILogger<UmbracoRequestMiddleware> _logger;

        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IRequestCache _requestCache;
        private readonly PublishedSnapshotServiceEventHandler _publishedSnapshotServiceEventHandler;
        private readonly IEventAggregator _eventAggregator;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly UmbracoRequestPaths _umbracoRequestPaths;
        private readonly BackOfficeWebAssets _backOfficeWebAssets;
        private readonly SmidgeOptions _smidgeOptions;
        private readonly WebProfiler _profiler;

        private static bool s_cacheInitialized;
        private static bool s_cacheInitializedFlag = false;
        private static object s_cacheInitializedLock = new object();

#pragma warning disable IDE0044 // Add readonly modifier
        private static bool s_firstBackOfficeRequest;
        private static bool s_firstBackOfficeReqestFlag;
        private static object s_firstBackOfficeRequestLocker = new object();
#pragma warning restore IDE0044 // Add readonly modifier

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoRequestMiddleware"/> class.
        /// </summary>
        public UmbracoRequestMiddleware(
            ILogger<UmbracoRequestMiddleware> logger,
            IUmbracoContextFactory umbracoContextFactory,
            IRequestCache requestCache,
            PublishedSnapshotServiceEventHandler publishedSnapshotServiceEventHandler,
            IEventAggregator eventAggregator,
            IProfiler profiler,
            IHostingEnvironment hostingEnvironment,
            UmbracoRequestPaths umbracoRequestPaths,
            BackOfficeWebAssets backOfficeWebAssets,
            IOptions<SmidgeOptions> smidgeOptions)
        {
            _logger = logger;
            _umbracoContextFactory = umbracoContextFactory;
            _requestCache = requestCache;
            _publishedSnapshotServiceEventHandler = publishedSnapshotServiceEventHandler;
            _eventAggregator = eventAggregator;
            _hostingEnvironment = hostingEnvironment;
            _umbracoRequestPaths = umbracoRequestPaths;
            _backOfficeWebAssets = backOfficeWebAssets;
            _smidgeOptions = smidgeOptions.Value;
            _profiler = profiler as WebProfiler; // Ignore if not a WebProfiler
        }

        /// <inheritdoc/>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // do not process if client-side request
            if (context.Request.IsClientSideRequest())
            {
                // we need this here because for bundle requests, these are 'client side' requests that we need to handle
                LazyInitializeBackOfficeServices(context.Request.Path);
                await next(context);
                return;
            }

            // Profiling start needs to be one of the first things that happens.
            // Also MiniProfiler.Current becomes null if it is handled by the event aggregator due to async/await
            _profiler?.UmbracoApplicationBeginRequest(context);

            EnsureContentCacheInitialized();

            UmbracoContextReference umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();

            Uri currentApplicationUrl = GetApplicationUrlFromCurrentRequest(context.Request);
            _hostingEnvironment.EnsureApplicationMainUrl(currentApplicationUrl);

            var pathAndQuery = context.Request.GetEncodedPathAndQuery();

            try
            {
                // Verbose log start of every request
                LogHttpRequest.TryGetCurrentHttpRequestId(out Guid httpRequestId, _requestCache);
                _logger.LogTrace("Begin request [{HttpRequestId}]: {RequestUrl}", httpRequestId, pathAndQuery);

                try
                {
                    LazyInitializeBackOfficeServices(context.Request.Path);
                    await _eventAggregator.PublishAsync(new UmbracoRequestBegin(umbracoContextReference.UmbracoContext));
                }
                catch (Exception ex)
                {
                    // try catch so we don't kill everything in all requests
                    _logger.LogError(ex.Message);
                }
                finally
                {
                    try
                    {
                        await next(context);

                    }
                    finally
                    {
                        await _eventAggregator.PublishAsync(new UmbracoRequestEnd(umbracoContextReference.UmbracoContext));
                    }
                }
            }
            finally
            {
                // Verbose log end of every request (in v8 we didn't log the end request of ALL requests, only the front-end which was
                // strange since we always logged the beginning, so now we just log start/end of all requests)
                LogHttpRequest.TryGetCurrentHttpRequestId(out Guid httpRequestId, _requestCache);
                _logger.LogTrace("End Request [{HttpRequestId}]: {RequestUrl} ({RequestDuration}ms)", httpRequestId, pathAndQuery, DateTime.Now.Subtract(umbracoContextReference.UmbracoContext.ObjectCreated).TotalMilliseconds);

                try
                {
                    DisposeHttpContextItems(context.Request);
                }
                finally
                {
                    // Dispose the umbraco context reference which will in turn dispose the UmbracoContext itself.
                    umbracoContextReference.Dispose();
                }
            }

            // Profiling end needs to be last of the first things that happens.
            // Also MiniProfiler.Current becomes null if it is handled by the event aggregator due to async/await
            _profiler?.UmbracoApplicationEndRequest(context);
        }

        /// <summary>
        /// Used to lazily initialize any back office services when the first request to the back office is made
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <returns></returns>
        private void LazyInitializeBackOfficeServices(PathString absPath)
        {
            if (s_firstBackOfficeRequest)
            {
                return;
            }

            if (_umbracoRequestPaths.IsBackOfficeRequest(absPath)
                || absPath.Value.InvariantStartsWith($"/{_smidgeOptions.UrlOptions.CompositeFilePath}")
                || absPath.Value.InvariantStartsWith($"/{_smidgeOptions.UrlOptions.BundleFilePath}"))
            {
                LazyInitializer.EnsureInitialized(ref s_firstBackOfficeRequest, ref s_firstBackOfficeReqestFlag, ref s_firstBackOfficeRequestLocker, () =>
                {
                    _backOfficeWebAssets.CreateBundles();
                    return true;
                });
            }
        }

        private Uri GetApplicationUrlFromCurrentRequest(HttpRequest request)
        {
            // We only consider GET and POST.
            // Especially the DEBUG sent when debugging the application is annoying because it uses http, even when the https is available.
            if (request.Method == "GET" || request.Method == "POST")
            {
                return new Uri($"{request.Scheme}://{request.Host}{request.PathBase}", UriKind.Absolute);

            }
            return null;
        }

        /// <summary>
        /// Dispose some request scoped objects that we are maintaining the lifecycle for.
        /// </summary>
        private void DisposeHttpContextItems(HttpRequest request)
        {
            // do not process if client-side request
            if (request.IsClientSideRequest())
            {
                return;
            }

            // ensure this is disposed by DI at the end of the request
            IHttpScopeReference httpScopeReference = request.HttpContext.RequestServices.GetRequiredService<IHttpScopeReference>();            
            httpScopeReference.Register();
        }

        /// <summary>
        /// Initializes the content cache one time
        /// </summary>
        private void EnsureContentCacheInitialized() => LazyInitializer.EnsureInitialized(
            ref s_cacheInitialized,
            ref s_cacheInitializedFlag,
            ref s_cacheInitializedLock,
            () =>
            {
                _publishedSnapshotServiceEventHandler.Initialize();
                return true;
            });
    }
}
