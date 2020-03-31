using System;
using Microsoft.AspNetCore.Http;

namespace Umbraco.Web.Common.Lifetime
{
    public class UmbracoRequestLifetime : IUmbracoRequestLifetimeManager
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
}
