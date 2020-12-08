using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Web.Common.Lifetime;

namespace Umbraco.Web.Common.Middleware
{

    /// <summary>
    /// Manages Umbraco request objects and their lifetime
    /// </summary>
    /// <remarks>
    /// This is responsible for creating and assigning an <see cref="IUmbracoContext"/>
    /// </remarks>
    public class UmbracoRequestMiddleware : IMiddleware
    {
        private readonly ILogger<UmbracoRequestMiddleware> _logger;
        private readonly IUmbracoRequestLifetimeManager _umbracoRequestLifetimeManager;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IRequestCache _requestCache;
        private readonly IBackOfficeSecurityFactory _backofficeSecurityFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoRequestMiddleware"/> class.
        /// </summary>
        public UmbracoRequestMiddleware(
            ILogger<UmbracoRequestMiddleware> logger,
            IUmbracoRequestLifetimeManager umbracoRequestLifetimeManager,
            IUmbracoContextFactory umbracoContextFactory,
            IRequestCache requestCache,
            IBackOfficeSecurityFactory backofficeSecurityFactory)
        {
            _logger = logger;
            _umbracoRequestLifetimeManager = umbracoRequestLifetimeManager;
            _umbracoContextFactory = umbracoContextFactory;
            _requestCache = requestCache;
            _backofficeSecurityFactory = backofficeSecurityFactory;
        }

        /// <inheritdoc/>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var requestUri = new Uri(context.Request.GetEncodedUrl(), UriKind.RelativeOrAbsolute);

            // do not process if client-side request
            if (requestUri.IsClientSideRequest())
            {
                await next(context);
                return;
            }

            _backofficeSecurityFactory.EnsureBackOfficeSecurity();  // Needs to be before UmbracoContext
            UmbracoContextReference umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();

            try
            {
                if (umbracoContextReference.UmbracoContext.IsFrontEndUmbracoRequest)
                {
                    LogHttpRequest.TryGetCurrentHttpRequestId(out Guid httpRequestId, _requestCache);
                    _logger.LogTrace("Begin request [{HttpRequestId}]: {RequestUrl}", httpRequestId, requestUri);
                }

                try
                {
                    _umbracoRequestLifetimeManager.InitRequest(context);
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
                        _umbracoRequestLifetimeManager.EndRequest(context);
                    }
                }
            }
            finally
            {
                if (umbracoContextReference.UmbracoContext.IsFrontEndUmbracoRequest)
                {
                    LogHttpRequest.TryGetCurrentHttpRequestId(out var httpRequestId, _requestCache);
                    _logger.LogTrace("End Request [{HttpRequestId}]: {RequestUrl} ({RequestDuration}ms)", httpRequestId, requestUri, DateTime.Now.Subtract(umbracoContextReference.UmbracoContext.ObjectCreated).TotalMilliseconds);
                }

                try
                {
                    DisposeRequestCacheItems(_logger, _requestCache, requestUri);
                }
                finally
                {
                    umbracoContextReference.Dispose();
                }
            }
        }

        /// <summary>
        /// Any object that is in the HttpContext.Items collection that is IDisposable will get disposed on the end of the request
        /// </summary>
        /// <param name="http"></param>
        /// <param name="requestCache"></param>
        /// <param name="requestUri"></param>
        private static void DisposeRequestCacheItems(ILogger<UmbracoRequestMiddleware> logger, IRequestCache requestCache, Uri requestUri)
        {
            // do not process if client-side request
            if (requestUri.IsClientSideRequest())
                return;

            //get a list of keys to dispose
            var keys = new HashSet<string>();
            foreach (var i in requestCache)
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


    }
}
