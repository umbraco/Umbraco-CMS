using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Umbraco.Web.Common.Extensions;
using Umbraco.Web.Common.Lifetime;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Common.AspNetCore
{
    public class AspNetCoreRequestAccessor : IRequestAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public AspNetCoreRequestAccessor(IHttpContextAccessor httpContextAccessor, IUmbracoRequestLifetime umbracoRequestLifetime, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _umbracoContextAccessor = umbracoContextAccessor;

            umbracoRequestLifetime.RequestStart += RequestStart;
            umbracoRequestLifetime.RequestEnd += RequestEnd;
        }

        private void RequestEnd(object sender, HttpContext e)
        {
            EndRequest?.Invoke(sender, new UmbracoRequestEventArgs(_umbracoContextAccessor.UmbracoContext));
        }

        private void RequestStart(object sender, HttpContext e)
        {
            var reason = EnsureRoutableOutcome.IsRoutable;
            RouteAttempt?.Invoke(sender, new RoutableAttemptEventArgs(reason, _umbracoContextAccessor.UmbracoContext));
        }



        public string GetRequestValue(string name) => GetFormValue(name) ?? GetQueryStringValue(name);
        public string GetFormValue(string name) => _httpContextAccessor.GetRequiredHttpContext().Request.Form[name];

        public string GetQueryStringValue(string name) => _httpContextAccessor.GetRequiredHttpContext().Request.Query[name];

        public event EventHandler<UmbracoRequestEventArgs> EndRequest;

        //TODO implement
        public event EventHandler<RoutableAttemptEventArgs> RouteAttempt;
        public Uri GetRequestUrl() => _httpContextAccessor.HttpContext != null ? new Uri(_httpContextAccessor.HttpContext.Request.GetEncodedUrl()) : null;
    }
}
