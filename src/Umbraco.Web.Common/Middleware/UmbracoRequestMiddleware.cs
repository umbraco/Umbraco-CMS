using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.PublishedCache;
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
        private readonly WebProfiler _profiler;
        private static bool s_cacheInitialized = false;
        private static bool s_cacheInitializedFlag = false;
        private static object s_cacheInitializedLock = new object();

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
            IHostingEnvironment hostingEnvironment)
        {
            _logger = logger;
            _umbracoContextFactory = umbracoContextFactory;
            _requestCache = requestCache;
            _publishedSnapshotServiceEventHandler = publishedSnapshotServiceEventHandler;
            _eventAggregator = eventAggregator;
            _hostingEnvironment = hostingEnvironment;
            _profiler = profiler as WebProfiler; // Ignore if not a WebProfiler
        }

        /// <inheritdoc/>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // do not process if client-side request
            if (context.Request.IsClientSideRequest())
            {
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
                    DisposeRequestCacheItems(_logger, _requestCache, context.Request);
                }
                finally
                {
                    umbracoContextReference.Dispose();
                }
            }

            // Profiling end needs to be last of the first things that happens.
            // Also MiniProfiler.Current becomes null if it is handled by the event aggregator due to async/await
            _profiler?.UmbracoApplicationEndRequest(context);
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
        /// Any object that is in the HttpContext.Items collection that is IDisposable will get disposed on the end of the request
        /// </summary>
        private static void DisposeRequestCacheItems(ILogger<UmbracoRequestMiddleware> logger, IRequestCache requestCache, HttpRequest request)
        {
            // do not process if client-side request
            if (request.IsClientSideRequest())
            {
                return;
            }

            // get a list of keys to dispose
            var keys = new HashSet<string>();
            foreach (var i in requestCache)
            {
                if (i.Value is IDisposeOnRequestEnd || i.Key is IDisposeOnRequestEnd)
                {
                    keys.Add(i.Key);
                }
            }

            // dispose each item and key that was found as disposable.
            foreach (var k in keys)
            {
                try
                {
                    requestCache.Get(k).DisposeIfDisposable();
                }
                catch (Exception ex)
                {
                    logger.LogError("Could not dispose item with key " + k, ex);
                }

                try
                {
                    k.DisposeIfDisposable();
                }
                catch (Exception ex)
                {
                    logger.LogError("Could not dispose item key " + k, ex);
                }
            }
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
