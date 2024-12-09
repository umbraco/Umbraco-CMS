using System.Security.Claims;
using Umbraco.Cms.Api.Management.ServerEvents.Models;

namespace Umbraco.Cms.Api.Management.Routing;

public interface IServerEventRouter
{
    Task RouteEventAsync(ServerEvent serverEvent);

    internal Task AssignToGroups(ClaimsPrincipal user, string connectionId);
}
