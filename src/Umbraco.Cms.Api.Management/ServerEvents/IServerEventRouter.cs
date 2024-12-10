using Umbraco.Cms.Api.Management.ServerEvents.Models;

namespace Umbraco.Cms.Api.Management.ServerEvents;

public interface IServerEventRouter
{
    Task RouteEventAsync(ServerEvent serverEvent);
}
