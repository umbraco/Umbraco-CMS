using System;
using Umbraco.Web.Routing;

namespace Umbraco.Web
{
    public interface IRequestAccessor
    {
        string GetRequestValue(string name);
        string GetQueryStringValue(string name);
        event EventHandler<UmbracoRequestEventArgs> EndRequest;
        event EventHandler<RoutableAttemptEventArgs> RouteAttempt;
        Uri GetRequestUrl();
    }
}
