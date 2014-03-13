using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;

namespace Umbraco.Web.WebApi.Filters
{
    public class DisableBrowserCacheAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            //See: http://stackoverflow.com/questions/17755239/how-to-stop-chrome-from-caching-rest-response-from-webapi

            base.OnActionExecuted(actionExecutedContext);

            //TODO: This should all work without issue! BUT it doesn't, i have a feeling this might be fixed
            // in the next webapi version. ASP.Net is overwriting the cachecontrol all the time, some docs are here:
            // http://stackoverflow.com/questions/11547618/output-caching-for-an-apicontroller-mvc4-web-api
            // and I've checked the source code so doing this should cause it to write the headers we want but it doesnt.
            //So I've reverted to brute force on the HttpContext.
            //actionExecutedContext.Response.Headers.CacheControl = new CacheControlHeaderValue()
            //{
            //    NoCache = true,
            //    NoStore = true,
            //    MaxAge = new TimeSpan(0),
            //    MustRevalidate = true
            //};

            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Current.Response.Cache.SetMaxAge(TimeSpan.Zero);
            HttpContext.Current.Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            HttpContext.Current.Response.Cache.SetNoStore();

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
