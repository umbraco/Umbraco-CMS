﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Umbraco.Web.Common.Lifetime;
using Umbraco.Core;
using Umbraco.Core.Logging;
using System.Threading;
using Umbraco.Core.Cache;
using System.Collections.Generic;
using Umbraco.Core.Security;

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
        private readonly ILogger _logger;
        private readonly IUmbracoRequestLifetimeManager _umbracoRequestLifetimeManager;
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IRequestCache _requestCache;
        private readonly IBackofficeSecurityFactory _backofficeSecurityFactory;

        public UmbracoRequestMiddleware(
            ILogger logger,
            IUmbracoRequestLifetimeManager umbracoRequestLifetimeManager,
            IUmbracoContextFactory umbracoContextFactory,
            IRequestCache requestCache,
            IBackofficeSecurityFactory backofficeSecurityFactory)
        {
            _logger = logger;
            _umbracoRequestLifetimeManager = umbracoRequestLifetimeManager;
            _umbracoContextFactory = umbracoContextFactory;
            _requestCache = requestCache;
            _backofficeSecurityFactory = backofficeSecurityFactory;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var requestUri = new Uri(context.Request.GetEncodedUrl(), UriKind.RelativeOrAbsolute);

            // do not process if client-side request
            if (requestUri.IsClientSideRequest())
            {
                await next(context);
                return;
            }
            _backofficeSecurityFactory.EnsureBackofficeSecurity();  // Needs to be before UmbracoContext
            var umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();


            try
            {
                if (umbracoContextReference.UmbracoContext.IsFrontEndUmbracoRequest)
                {
                    LogHttpRequest.TryGetCurrentHttpRequestId(out var httpRequestId, _requestCache);
                   _logger.Verbose<UmbracoRequestMiddleware>("Begin request [{HttpRequestId}]: {RequestUrl}", httpRequestId, requestUri);
                }

                try
                {
                    _umbracoRequestLifetimeManager.InitRequest(context);
                }
                catch (Exception ex)
                {
                    // try catch so we don't kill everything in all requests
                    _logger.Error<UmbracoRequestMiddleware>(ex);
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
                    _logger.Verbose<UmbracoRequestMiddleware>("End Request [{HttpRequestId}]: {RequestUrl} ({RequestDuration}ms)", httpRequestId, requestUri, DateTime.Now.Subtract(umbracoContextReference.UmbracoContext.ObjectCreated).TotalMilliseconds);
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
        private static void DisposeRequestCacheItems(ILogger logger, IRequestCache requestCache, Uri requestUri)
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
                    logger.Error<UmbracoRequestMiddleware>("Could not dispose item with key " + k, ex);
                }
                try
                {
                    k.DisposeIfDisposable();
                }
                catch (Exception ex)
                {
                    logger.Error<UmbracoRequestMiddleware>("Could not dispose item key " + k, ex);
                }
            }
        }


    }
}
