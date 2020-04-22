using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Umbraco.Web.Common.Lifetime;

namespace Umbraco.Web.Common.Middleware
{
    public class UmbracoRequestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IUmbracoRequestLifetimeManager _umbracoRequestLifetimeManager;
        public UmbracoRequestMiddleware(RequestDelegate next, IUmbracoRequestLifetimeManager umbracoRequestLifetimeManager)
        {
            _next = next;
            _umbracoRequestLifetimeManager = umbracoRequestLifetimeManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _umbracoRequestLifetimeManager.InitRequest(context);
            await _next(context);
            _umbracoRequestLifetimeManager.EndRequest(context);
        }
    }
}
