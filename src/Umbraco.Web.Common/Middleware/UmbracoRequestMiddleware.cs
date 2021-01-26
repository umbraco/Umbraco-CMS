using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Extensions;
using Umbraco.Web.Common.Profiler;
using Umbraco.Web.PublishedCache.NuCache;

namespace Umbraco.Web.Common.Middleware
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
        private readonly IBackOfficeSecurityFactory _backofficeSecurityFactory;
        private readonly PublishedSnapshotServiceEventHandler _publishedSnapshotServiceEventHandler;
        private readonly IEventAggregator _eventAggregator;
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
            IBackOfficeSecurityFactory backofficeSecurityFactory,
            PublishedSnapshotServiceEventHandler publishedSnapshotServiceEventHandler,
            IEventAggregator eventAggregator,
            IProfiler profiler)
        {
            _logger = logger;
            _umbracoContextFactory = umbracoContextFactory;
            _requestCache = requestCache;
            _backofficeSecurityFactory = backofficeSecurityFactory;
            _publishedSnapshotServiceEventHandler = publishedSnapshotServiceEventHandler;
            _eventAggregator = eventAggregator;
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

            _backofficeSecurityFactory.EnsureBackOfficeSecurity();  // Needs to be before UmbracoContext, TODO: Why?
            UmbracoContextReference umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();

            bool isFrontEndRequest = umbracoContextReference.UmbracoContext.IsFrontEndUmbracoRequest();

            var pathAndQuery = context.Request.GetEncodedPathAndQuery();

            try
            {
                if (isFrontEndRequest)
                {
                    LogHttpRequest.TryGetCurrentHttpRequestId(out Guid httpRequestId, _requestCache);
                    _logger.LogTrace("Begin request [{HttpRequestId}]: {RequestUrl}", httpRequestId, pathAndQuery);
                }

                try
                {
                    await _eventAggregator.PublishAsync(new UmbracoRequestBegin(context));
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
                        await _eventAggregator.PublishAsync(new UmbracoRequestEnd(context));
                    }
                }
            }
            finally
            {
                if (isFrontEndRequest)
                {
                    LogHttpRequest.TryGetCurrentHttpRequestId(out var httpRequestId, _requestCache);
                    _logger.LogTrace("End Request [{HttpRequestId}]: {RequestUrl} ({RequestDuration}ms)", httpRequestId, pathAndQuery, DateTime.Now.Subtract(umbracoContextReference.UmbracoContext.ObjectCreated).TotalMilliseconds);
                }

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
