using System;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;

namespace Umbraco.Web.WebApi
{
    internal static class WebApiHelper
    {
        /// <summary>
        /// A helper method to create a WebAPI HttpControllerContext which can be used to execute a controller manually
        /// </summary>
        /// <param name="method"></param>
        /// <param name="uri"></param>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        internal static HttpControllerContext CreateContext(HttpMethod method, Uri uri, HttpContextBase httpContext)
        {
            var config = new HttpConfiguration(GlobalConfiguration.Configuration.Routes);
            IHttpRouteData route = new HttpRouteData(new HttpRoute());
            var req = new HttpRequestMessage(method, uri);
            req.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            req.Properties[HttpPropertyKeys.HttpRouteDataKey] = route;
            req.Properties["MS_HttpContext"] = httpContext;
            return new HttpControllerContext(config, route, req);
        }
    }
}