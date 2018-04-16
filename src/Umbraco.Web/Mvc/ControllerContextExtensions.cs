using System.Web.Mvc;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Mvc
{
    public static class ControllerContextExtensions
    {
        /// <summary>
        /// Gets the Umbraco context from a controller context hierarchy, if any, else the 'current' Umbraco context.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <returns>The Umbraco context.</returns>
        public static UmbracoContext GetUmbracoContext(this ControllerContext controllerContext)
        {
            var o = controllerContext.GetDataTokenInViewContextHierarchy(Core.Constants.Web.UmbracoContextDataToken);
            return o != null ? o as UmbracoContext : Current.UmbracoContext;
        }

        /// <summary>
        /// Recursively gets a data token from a controller context hierarchy.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="dataTokenName">The name of the data token.</param>
        /// <returns>The data token, or null.</returns>
        internal static object GetDataTokenInViewContextHierarchy(this ControllerContext controllerContext, string dataTokenName)
        {
            var context = controllerContext;
            while (context != null)
            {
                object token;
                if (context.RouteData.DataTokens.TryGetValue(dataTokenName, out token))
                    return token;
                context = context.ParentActionViewContext;
            }
            return null;
        }
    }
}
