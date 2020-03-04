using System;
using Umbraco.Web.Routing;

namespace Umbraco.Core.Request
{
    public interface IRequestAccessor
    {
        string GetRequestValue(string name);
        string GetQueryStringValue(string culture);
        event EventHandler<UmbracoRequestEventArgs> EndRequest;
        event EventHandler<RoutableAttemptEventArgs> RouteAttempt;
    }
}
