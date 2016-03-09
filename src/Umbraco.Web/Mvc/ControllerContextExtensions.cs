using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
    public static class ControllerContextExtensions
    {
        /// <summary>
        /// Tries to get the Umbraco context from the whole ControllerContext hierarchy based on data tokens and if that fails
        /// it will attempt to fallback to retrieving it from the HttpContext.
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <returns></returns>
        public static UmbracoContext GetUmbracoContext(this ControllerContext controllerContext)
        {
            var umbCtx = controllerContext.RouteData.GetUmbracoContext();
            if (umbCtx != null) return umbCtx;

            if (controllerContext.ParentActionViewContext != null)
            {
                //recurse
                return controllerContext.ParentActionViewContext.GetUmbracoContext();
            }

            //fallback to getting from HttpContext
            return controllerContext.HttpContext.GetUmbracoContext();
        }

        /// <summary>
        /// Find a data token in the whole ControllerContext hierarchy of execution
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="dataTokenName"></param>
        /// <returns></returns>
        internal static object GetDataTokenInViewContextHierarchy(this ControllerContext controllerContext, string dataTokenName)
        {
            if (controllerContext.RouteData.DataTokens.ContainsKey(dataTokenName))
            {
                return controllerContext.RouteData.DataTokens[dataTokenName];
            }

            if (controllerContext.ParentActionViewContext != null)
            {
                //recurse!
                return controllerContext.ParentActionViewContext.GetDataTokenInViewContextHierarchy(dataTokenName);
            }

            return null;
        }
    }
}