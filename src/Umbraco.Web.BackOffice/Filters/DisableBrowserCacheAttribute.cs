using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// Ensures that the request is not cached by the browser
    /// </summary>
    public class DisableBrowserCacheAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.HttpContext?.Response?.StatusCode != 200)
            {
                return;
            }

            context.HttpContext.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            context.HttpContext.Response.Headers["Expires"] = "-1";
            context.HttpContext.Response.Headers["Pragma"] = "no-cache";
        }
    }
}
