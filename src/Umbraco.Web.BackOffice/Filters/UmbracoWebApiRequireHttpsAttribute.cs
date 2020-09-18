using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// If Umbraco.Core.UseHttps property in web.config is set to true, this filter will redirect any http access to https.
    /// </summary>
    /// <remarks>
    /// This will only redirect Head/Get requests, otherwise will respond with text
    ///
    /// References:
    /// http://issues.umbraco.org/issue/U4-8542
    /// https://blogs.msdn.microsoft.com/carlosfigueira/2012/03/09/implementing-requirehttps-with-asp-net-web-api/
    /// </remarks>
    public class UmbracoWebApiRequireHttpsAttribute : TypeFilterAttribute
    {
        public UmbracoWebApiRequireHttpsAttribute() : base(typeof(UmbracoWebApiRequireHttpsFilter))
        {
            Arguments = Array.Empty<object>();
        }
    }

    public  class UmbracoWebApiRequireHttpsFilter: IAuthorizationFilter
    {
        private readonly GlobalSettings _globalSettings;

        public UmbracoWebApiRequireHttpsFilter(IOptions<GlobalSettings> globalSettings)
        {
            _globalSettings = globalSettings.Value;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            if (_globalSettings.UseHttps && request.Scheme != Uri.UriSchemeHttps)
            {
                var uri = new UriBuilder()
                {
                    Scheme = Uri.UriSchemeHttps,
                    Host = request.Host.Value,
                    Path = request.Path,
                    Query = request.QueryString.ToUriComponent(),
                    Port = 443
                };
                var body = string.Format("<p>The resource can be found at <a href =\"{0}\">{0}</a>.</p>",
                    uri.Uri.AbsoluteUri);
                if (request.Method.Equals(HttpMethod.Get.ToString()) || request.Method.Equals(HttpMethod.Head.ToString()))
                {
                    context.HttpContext.Response.Headers.Add("Location", uri.Uri.ToString());
                    context.Result = new ObjectResult(body)
                    {
                        StatusCode = (int)HttpStatusCode.Found,
                    };

                }
                else
                {
                    context.Result = new ObjectResult(body)
                    {
                        StatusCode = (int)HttpStatusCode.NotFound
                    };
                }


            }
        }
    }
}
