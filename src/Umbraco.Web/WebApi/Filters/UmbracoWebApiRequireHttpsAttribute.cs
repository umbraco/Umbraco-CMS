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
        private readonly UmbracoContext _umbracoContext;

        public UmbracoWebApiRequireHttpsAttribute()
        {
        }

        /// <summary>
        /// THIS SHOULD BE ONLY USED FOR UNIT TESTS
        /// </summary>
        /// <param name="umbracoContext"></param>
        public UmbracoWebApiRequireHttpsAttribute(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            _umbracoContext = umbracoContext;
        }

        private UmbracoContext GetUmbracoContext()
        {
            return _umbracoContext ?? UmbracoContext.Current;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var request = actionContext.Request;            

            if (GlobalSettings.UseSSL)
            {
                var umbCtx = GetUmbracoContext();
                if (umbCtx == null)
                    throw new InvalidOperationException("No UmbracoContext was found in the request");

                var httpCtx = actionContext.Request.TryGetHttpContext();
                if (httpCtx.Success == false)
                    throw new InvalidOperationException("No HttpContext was found in the request");

                if (umbCtx.SecureRequest.IsSecure(httpCtx.Result.Request))
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
}