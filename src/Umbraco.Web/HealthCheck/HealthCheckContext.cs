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
            if (httpContext == null) throw new ArgumentNullException("httpContext");
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            HttpContext = httpContext;
            UmbracoContext = umbracoContext;
            ApplicationContext = UmbracoContext.Application;
        }

        public HttpContextBase HttpContext { get; private set; }
        public UmbracoContext UmbracoContext { get; private set; }
        public ApplicationContext ApplicationContext { get; private set; }

        //TODO: Do we need any more info/service exposed here?
    }
}