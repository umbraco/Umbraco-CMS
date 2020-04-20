using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// Ensures that the request is not cached by the browser
    /// </summary>
    public class DisableBrowserCacheAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            base.OnResultExecuting(context);

            if (context.HttpContext.Response.StatusCode != 200) return;

            context.HttpContext.Response.GetTypedHeaders().CacheControl =
                new CacheControlHeaderValue()
                {
                    NoCache = true,
                    MaxAge = TimeSpan.Zero,
                    MustRevalidate = true,
                    NoStore = true
                };

            context.HttpContext.Response.Headers[HeaderNames.LastModified] = DateTime.Now.ToString("R"); // Format RFC1123
            context.HttpContext.Response.Headers[HeaderNames.Pragma] = "no-cache";
            context.HttpContext.Response.Headers[HeaderNames.Expires] = new DateTime(1990, 1, 1, 0, 0, 0).ToString("R");
        }
    }
}
