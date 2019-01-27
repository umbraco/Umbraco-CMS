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
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);

            // could happens if exception (but AFAIK this wouldn't happen in MVC)
            if (filterContext.HttpContext == null || filterContext.HttpContext.Response == null ||
                    filterContext.HttpContext.Response.Cache == null)
            {
                return;
            }

            if (filterContext.IsChildAction)
            {
                return;
            }

            if (filterContext.HttpContext.Response.StatusCode != 200)
            {
                return;
            }

            filterContext.HttpContext.Response.Cache.SetLastModified(DateTime.Now);
            filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            filterContext.HttpContext.Response.Cache.SetMaxAge(TimeSpan.Zero);
            filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            filterContext.HttpContext.Response.Cache.SetNoStore();
            filterContext.HttpContext.Response.AddHeader("Pragma", "no-cache");
            filterContext.HttpContext.Response.Cache.SetExpires(new DateTime(1990, 1, 1, 0, 0, 0));
        }
    }
}
