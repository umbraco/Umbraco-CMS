using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Events;
using Umbraco.Extensions;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Common.AspNetCore
{
    public class AspNetCoreRequestAccessor : IRequestAccessor, INotificationHandler<UmbracoRequestBegin>,  INotificationHandler<UmbracoRequestEnd>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly WebRoutingSettings _webRoutingSettings;
        private readonly ISet<string> _applicationUrls = new HashSet<string>();
        private Uri _currentApplicationUrl;

        public AspNetCoreRequestAccessor(IHttpContextAccessor httpContextAccessor,
            IUmbracoContextAccessor umbracoContextAccessor,
            IOptions<WebRoutingSettings> webRoutingSettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _umbracoContextAccessor = umbracoContextAccessor;
            _webRoutingSettings = webRoutingSettings.Value;

        }


        public string GetRequestValue(string name) => GetFormValue(name) ?? GetQueryStringValue(name);

        public string GetFormValue(string name)
        {
            var request = _httpContextAccessor.GetRequiredHttpContext().Request;
            if (!request.HasFormContentType) return null;
            return request.Form[name];
        }

        public string GetQueryStringValue(string name) => _httpContextAccessor.GetRequiredHttpContext().Request.Query[name];

        public event EventHandler<UmbracoRequestEventArgs> EndRequest;

        public event EventHandler<RoutableAttemptEventArgs> RouteAttempt;

        public Uri GetRequestUrl() => _httpContextAccessor.HttpContext != null ? new Uri(_httpContextAccessor.HttpContext.Request.GetEncodedUrl()) : null;

        public Uri GetApplicationUrl()
        {
            // Fixme: This causes problems with site swap on azure because azure pre-warms a site by calling into `localhost` and when it does that
            // it changes the URL to `localhost:80` which actually doesn't work for pinging itself, it only works internally in Azure. The ironic part
            // about this is that this is here specifically for the slot swap scenario https://issues.umbraco.org/issue/U4-10626

            // see U4-10626 - in some cases we want to reset the application url
            // (this is a simplified version of what was in 7.x)
            // note: should this be optional? is it expensive?

            if (!(_webRoutingSettings.UmbracoApplicationUrl is null))
            {
                return new Uri(_webRoutingSettings.UmbracoApplicationUrl);
            }

            var request = _httpContextAccessor.HttpContext?.Request;

            if (request is null)
            {
                return _currentApplicationUrl;
            }

            var url = UriHelper.BuildAbsolute(request.Scheme, request.Host);
            var change = url != null && !_applicationUrls.Contains(url);
            if (change)
            {
                _applicationUrls.Add(url);

                _currentApplicationUrl ??= new Uri(url);
            }

            return _currentApplicationUrl;
        }

        public void Handle(UmbracoRequestBegin notification)
        {
            var reason = EnsureRoutableOutcome.IsRoutable; //TODO get the correct value here like in UmbracoInjectedModule
            RouteAttempt?.Invoke(this, new RoutableAttemptEventArgs(reason, _umbracoContextAccessor.UmbracoContext));
        }

        public void Handle(UmbracoRequestEnd notification)
        {
            EndRequest?.Invoke(this, new UmbracoRequestEventArgs(_umbracoContextAccessor.UmbracoContext));
        }


    }
}
