using System;
using System.Net;
using System.Net.Http;
using System.Web;
using Microsoft.Owin;
using Umbraco.Cms.Core;

namespace Umbraco.Web.WebApi
{

    public static class HttpRequestMessageExtensions
    {

        /// <summary>
        /// Borrowed from the latest Microsoft.AspNet.WebApi.Owin package which we cannot use because of a later webapi dependency
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal static Attempt<IOwinContext> TryGetOwinContext(this HttpRequestMessage request)
        {
            // occurs in unit tests?
            if (request.Properties.TryGetValue("MS_OwinContext", out var o) && o is IOwinContext owinContext)
                return Attempt.Succeed(owinContext);

            var httpContext = request.TryGetHttpContext();
            try
            {
                return httpContext
                        ? Attempt.Succeed(httpContext.Result.GetOwinContext())
                        : Attempt<IOwinContext>.Fail();
            }
            catch (InvalidOperationException)
            {
                //this will occur if there is no OWIN environment which generally would only be in things like unit tests
                return Attempt<IOwinContext>.Fail();
            }
        }

        /// <summary>
        /// Tries to retrieve the current HttpContext if one exists.
        /// </summary>
        /// <returns></returns>
        public static Attempt<HttpContextBase> TryGetHttpContext(this HttpRequestMessage request)
        {
            object context;
            if (request.Properties.TryGetValue("MS_HttpContext", out context))
            {
                var httpContext = context as HttpContextBase;
                if (httpContext != null)
                {
                    return Attempt.Succeed(httpContext);
                }
            }
            if (HttpContext.Current != null)
            {
                return Attempt<HttpContextBase>.Succeed(new HttpContextWrapper(HttpContext.Current));
            }

            return Attempt<HttpContextBase>.Fail();
        }


    }

}
