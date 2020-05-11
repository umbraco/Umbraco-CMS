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

        // TODO: Not sure this belongs here but we can leave it for now
        Uri GetApplicationUrl();
    }
}
