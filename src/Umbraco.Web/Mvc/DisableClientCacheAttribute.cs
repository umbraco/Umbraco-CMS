using System;
using System.Web;
using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Will ensure that client-side cache does not occur by sending the correct response headers
    /// </summary>    
    public class DisableClientCacheAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.IsChildAction) base.OnResultExecuting(filterContext);

            filterContext.HttpContext.Response.Cache.SetExpires(DateTime.Now.AddDays(-10));
            filterContext.HttpContext.Response.Cache.SetLastModified(DateTime.Now);
            filterContext.HttpContext.Response.Cache.SetValidUntilExpires(false);
            filterContext.HttpContext.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            filterContext.HttpContext.Response.Cache.SetNoStore();

            base.OnResultExecuting(filterContext);
        }
    }
}