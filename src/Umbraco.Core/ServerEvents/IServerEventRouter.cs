using Umbraco.Cms.Core.Models.ServerEvents;

namespace Umbraco.Cms.Core.ServerEvents;

public interface IServerEventRouter
{
    Task RouteEventAsync(ServerEvent serverEvent);
}
