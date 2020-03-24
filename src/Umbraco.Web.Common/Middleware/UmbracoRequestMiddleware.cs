using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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

    public interface IUmbracoRequestLifetime
    {
        event EventHandler<HttpContext> RequestStart;
        event EventHandler<HttpContext> RequestEnd;
    }

    public class UmbracoRequestLifetime : IUmbracoRequestLifetime, IUmbracoRequestLifetimeManager
    {
        public event EventHandler<HttpContext> RequestStart;
        public event EventHandler<HttpContext> RequestEnd;

        public void InitRequest(HttpContext context)
        {
            RequestStart?.Invoke(this, context);
        }

        public void EndRequest(HttpContext context)
        {
            RequestEnd?.Invoke(this, context);
        }
    }

    public interface IUmbracoRequestLifetimeManager
    {
        void InitRequest(HttpContext context);
        void EndRequest(HttpContext context);
    }
}
