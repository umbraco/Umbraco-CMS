using System;
using Microsoft.AspNetCore.Http;
using Umbraco.Web.Common.Extensions;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Common.AspNetCore
{
    public class AspNetCoreRequestAccessor : IRequestAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AspNetCoreRequestAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetRequestValue(string name) => GetFormValue(name) ?? GetQueryStringValue(name);
        public string GetFormValue(string name) => _httpContextAccessor.GetRequiredHttpContext().Request.Form[name];

        public string GetQueryStringValue(string name) => _httpContextAccessor.GetRequiredHttpContext().Request.Query[name];

        //TODO implement
        public event EventHandler<UmbracoRequestEventArgs> EndRequest;

        //TODO implement
        public event EventHandler<RoutableAttemptEventArgs> RouteAttempt;
    }
}
