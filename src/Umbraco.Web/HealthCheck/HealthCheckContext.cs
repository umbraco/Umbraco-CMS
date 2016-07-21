using System;
using System.Web;
using Umbraco.Core;

namespace Umbraco.Web.HealthCheck
{
    /// <summary>
    /// Context exposing all services that could be required for health check classes to perform and/or fix their checks
    /// </summary>
    public class HealthCheckContext
    {
        public HealthCheckContext(HttpContextBase httpContext, UmbracoContext umbracoContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));
            if (umbracoContext == null) throw new ArgumentNullException(nameof(umbracoContext));

            HttpContext = httpContext;
            UmbracoContext = umbracoContext;
        }

        public HttpContextBase HttpContext { get; }
        public UmbracoContext UmbracoContext { get; }
        public ApplicationContext ApplicationContext => UmbracoContext.Application;

        //TODO: Do we need any more info/service exposed here?
    }
}