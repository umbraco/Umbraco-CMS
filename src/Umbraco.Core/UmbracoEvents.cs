using System;
using Umbraco.Web.Routing;

namespace Umbraco.Core
{
    public interface IUmbracoRouteEventSender
    {
        event EventHandler<RoutableAttemptEventArgs> RouteAttempt;
    }
}
