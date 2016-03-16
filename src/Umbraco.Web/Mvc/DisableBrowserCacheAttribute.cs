using System;
using System.Web;
using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Ensures that the request is not cached by the browser
    /// </summary>
    public class DisableBrowserCacheAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);            
            filterContext.HttpContext.Response.Cache.SetMaxAge(TimeSpan.Zero);            
            filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            filterContext.HttpContext.Response.Cache.SetNoStore();
            filterContext.HttpContext.Response.AddHeader("Pragma", "no-cache");
            filterContext.HttpContext.Response.Cache.SetExpires(new DateTime(1990, 1, 1, 0, 0, 0));
        }
    }
}