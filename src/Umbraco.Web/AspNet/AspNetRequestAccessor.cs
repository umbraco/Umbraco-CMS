using System;
using Umbraco.Web.Routing;

namespace Umbraco.Web.AspNet
{
    public class AspNetRequestAccessor : IRequestAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AspNetRequestAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            UmbracoModule.EndRequest += OnEndRequest;
            UmbracoModule.RouteAttempt += OnRouteAttempt;
        }



        public string GetRequestValue(string name)
        {
            return _httpContextAccessor.GetRequiredHttpContext().Request[name];
        }

        public string GetQueryStringValue(string name)
        {
            return _httpContextAccessor.GetRequiredHttpContext().Request.QueryString[name];
        }

        private void OnEndRequest(object sender, UmbracoRequestEventArgs args)
        {
            EndRequest?.Invoke(sender, args);
        }

        private void OnRouteAttempt(object sender, RoutableAttemptEventArgs args)
        {
            RouteAttempt?.Invoke(sender, args);
        }
        public event EventHandler<UmbracoRequestEventArgs> EndRequest;
        public event EventHandler<RoutableAttemptEventArgs> RouteAttempt;
        public Uri GetRequestUrl() => _httpContextAccessor.HttpContext?.Request.Url;
    }
}
