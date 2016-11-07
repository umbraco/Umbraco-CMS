using System;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Ensures that the request is not cached by the browser
    /// </summary>
    public class DisableBrowserCacheAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            //See: http://stackoverflow.com/questions/17755239/how-to-stop-chrome-from-caching-rest-response-from-webapi

            base.OnActionExecuted(actionExecutedContext);
            if (actionExecutedContext == null || actionExecutedContext.Response == null ||
                    actionExecutedContext.Response.Headers == null)
            {
                return;
            }
            //NOTE: Until we upgraded to WebApi 2, this didn't work correctly and we had to revert to using
            // HttpContext.Current responses. I've changed this back to what it should be now since it works
            // and now with WebApi2, the HttpContext.Current responses dont! Anyways, all good now.
            actionExecutedContext.Response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                NoCache = true,
                NoStore = true,
                MaxAge = new TimeSpan(0),
                MustRevalidate = true
            };

            actionExecutedContext.Response.Headers.Pragma.Add(new NameValueHeaderValue("no-cache"));
            if (actionExecutedContext.Response.Content != null)
            {
                actionExecutedContext.Response.Content.Headers.Expires =
                        //Mon, 01 Jan 1990 00:00:00 GMT
                        new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero);
            }
        }
    }
}
