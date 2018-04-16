using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// If umbracoUseSSL property in web.config is set to true, this filter will redirect any http access to https.
    /// </summary>
    /// <remarks>
    /// This will only redirect Head/Get requests, otherwise will respond with text
    ///
    /// References:
    /// http://issues.umbraco.org/issue/U4-8542
    /// https://blogs.msdn.microsoft.com/carlosfigueira/2012/03/09/implementing-requirehttps-with-asp-net-web-api/
    /// </remarks>
    public class UmbracoWebApiRequireHttpsAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var request = actionContext.Request;
            if (UmbracoConfig.For.GlobalSettings().UseHttps && request.RequestUri.Scheme != Uri.UriSchemeHttps)
            {
                HttpResponseMessage response;
                var uri = new UriBuilder(request.RequestUri)
                {
                    Scheme = Uri.UriSchemeHttps,
                    Port = 443
                };
                var body = string.Format("<p>The resource can be found at <a href =\"{0}\">{0}</a>.</p>",
                uri.Uri.AbsoluteUri);
                if (request.Method.Equals(HttpMethod.Get) || request.Method.Equals(HttpMethod.Head))
                {
                    response = request.CreateResponse(HttpStatusCode.Found);
                    response.Headers.Location = uri.Uri;
                    if (request.Method.Equals(HttpMethod.Get))
                    {
                        response.Content = new StringContent(body, Encoding.UTF8, "text/html");
                    }
                }
                else
                {
                    response = request.CreateResponse(HttpStatusCode.NotFound);
                    response.Content = new StringContent(body, Encoding.UTF8, "text/html");
                }

                actionContext.Response = response;
            }
        }
    }
}
