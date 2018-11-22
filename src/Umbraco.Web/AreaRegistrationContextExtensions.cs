using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Umbraco.Web
{
    internal static class AreaRegistrationContextExtensions
    {
        public static Route MapHttpRoute(this AreaRegistrationContext context, string name, string url, object defaults, string[] namespaces)
        {
            var apiRoute = context.Routes.MapHttpRoute(
                name,
                url,
                defaults);

            //web api routes don't set the data tokens object
            if (apiRoute.DataTokens == null)
            {
                apiRoute.DataTokens = new RouteValueDictionary();
            }
            apiRoute.DataTokens.Add("area", context.AreaName);
            apiRoute.DataTokens.Add("Namespaces", namespaces); //look in this namespace to create the controller
            apiRoute.DataTokens.Add("UseNamespaceFallback", false); //Don't look anywhere else except this namespace!

            return apiRoute;
        }
    }
}