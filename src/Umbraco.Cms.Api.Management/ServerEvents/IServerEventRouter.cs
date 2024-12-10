using Umbraco.Cms.Api.Management.ServerEvents.Models;

namespace Umbraco.Cms.Api.Management.Routing;

public interface IServerEventRouter
{
    Task RouteEventAsync(ServerEvent serverEvent);
}
