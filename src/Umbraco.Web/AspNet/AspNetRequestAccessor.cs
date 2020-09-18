using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Web.Routing;

namespace Umbraco.Web.AspNet
{
    public class AspNetRequestAccessor : IRequestAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly WebRoutingSettings _webRoutingSettings;
        private readonly ISet<string> _applicationUrls = new HashSet<string>();
        private Uri _currentApplicationUrl;
        public AspNetRequestAccessor(IHttpContextAccessor httpContextAccessor, IOptions<WebRoutingSettings> webRoutingSettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _webRoutingSettings = webRoutingSettings.Value;

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
        public Uri GetApplicationUrl()
        {
            //Fixme: This causes problems with site swap on azure because azure pre-warms a site by calling into `localhost` and when it does that
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

            var url = request?.Url.GetLeftPart(UriPartial.Authority);
            var change = url != null && !_applicationUrls.Contains(url);
            if (change)
            {
                _applicationUrls.Add(url);

                _currentApplicationUrl ??= new Uri(url);
            }

            return _currentApplicationUrl;
        }
    }
}
