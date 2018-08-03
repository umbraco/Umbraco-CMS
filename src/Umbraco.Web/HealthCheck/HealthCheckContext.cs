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
        private readonly HttpContextBase _httpContext;

        private readonly UmbracoContext _umbracoContext;

        public HealthCheckContext(HttpContextBase httpContext, UmbracoContext umbracoContext)
        {
            if (httpContext == null) throw new ArgumentNullException("httpContext");
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            _httpContext = httpContext;
            _umbracoContext = umbracoContext;
            ApplicationContext = _umbracoContext.Application;
        }

        public HealthCheckContext(ApplicationContext applicationContext)
        {
            ApplicationContext = applicationContext;
        }

        public ApplicationContext ApplicationContext { get; private set; }

        public string SiteUrl
        {
            get
            {
                return _httpContext != null
                    ? _httpContext.Request.Url.GetLeftPart(UriPartial.Authority)
                    : ApplicationContext.UmbracoApplicationUrl.Replace("/umbraco", string.Empty);
            }
        }

        public string ApplicationPath
        {
            get
            {
                return _httpContext != null
                    ? _httpContext.Request.ApplicationPath
                    : "/";
            }
        }
    }
}