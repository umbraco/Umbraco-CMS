using System;
using System.Web.Routing;

namespace Umbraco.Web
{
    public static class RouteDataExtensions
    {
        /// <summary>
        /// Tries to get the Umbraco context from the DataTokens
        /// </summary>
        /// <param name="routeData"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is useful when working on async threads since the UmbracoContext is not copied over explicitly
        /// </remarks>
        public static UmbracoContext GetUmbracoContext(this RouteData routeData)
        {
            if (routeData == null) throw new ArgumentNullException("routeData");

            if (routeData.DataTokens.ContainsKey(Core.Constants.Web.UmbracoContextDataToken))
            {
                var umbCtx = routeData.DataTokens[Core.Constants.Web.UmbracoContextDataToken] as UmbracoContext;
                return umbCtx;
            }
            return null;
        }
    }
}