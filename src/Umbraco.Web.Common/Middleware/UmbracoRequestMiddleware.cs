using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Umbraco.Web.Common.Lifetime;
using Umbraco.Core;
using Umbraco.Core.Logging;
using System.Threading;

namespace Umbraco.Web.Common.Middleware
{

    /// <summary>
    /// Manages Umbraco request objects and their lifetime
    /// </summary>
    public class UmbracoRequestMiddleware : IMiddleware
    {
        private readonly ILogger _logger;
        private readonly IUmbracoRequestLifetimeManager _umbracoRequestLifetimeManager;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public UmbracoRequestMiddleware(
            ILogger logger,
            IUmbracoRequestLifetimeManager umbracoRequestLifetimeManager,
            IUmbracoContextFactory umbracoContextFactory)
        {
            _logger = logger;
            _umbracoRequestLifetimeManager = umbracoRequestLifetimeManager;
            _umbracoContextFactory = umbracoContextFactory;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // do not process if client-side request

            if (new Uri(context.Request.GetEncodedUrl(), UriKind.RelativeOrAbsolute).IsClientSideRequest())
            {
                await next(context);
                return;
            }

            var umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext();

            try
            {
                try
                {
                    _umbracoRequestLifetimeManager.InitRequest(context);
                }
                catch (Exception ex)
                {
                    // try catch so we don't kill everything in all requests
                    _logger.Error<UmbracoRequestMiddleware>(ex);
                }

                await next(context);

                _umbracoRequestLifetimeManager.EndRequest(context);
            }
            finally
            {
                umbracoContextReference.Dispose();
            }
        }
    }
}
