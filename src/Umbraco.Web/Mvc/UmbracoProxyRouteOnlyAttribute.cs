using System.Web;
using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// When applied to a <see cref="SurfaceController"/> does not let the controller execute by it's direct route, only when its routed internally by Umbraco
    /// </summary>
    public class UmbracoProxyRouteOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            var httpContext = filterContext.HttpContext;

            var ufprt = httpContext.Request["ufprt"];

            if (ufprt == null)
            {
                NotFound(filterContext);
                return;
            }
                

            if (!UmbracoHelper.DecryptAndValidateEncryptedRouteString(ufprt, out var parts))
            {
                NotFound(filterContext);
                return;
            }

            var rd = httpContext.Request.RequestContext.RouteData;
            var currentAction = rd.GetRequiredString("action");
            var currentController = rd.GetRequiredString("controller");

            if (currentController != parts[RenderRouteHandler.ReservedAdditionalKeys.Controller])
            {
                NotFound(filterContext);
                return;
            }

            if (currentAction != parts[RenderRouteHandler.ReservedAdditionalKeys.Action])
            {
                NotFound(filterContext);
                return;
            }
        }

        private void NotFound(ActionExecutingContext filterContext)
        {
            filterContext.Result = new HttpStatusCodeResult(404);
        }
    }
}
