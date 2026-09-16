using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.ServerEvents;

namespace Umbraco.Cms.Tests.Integration.Testing.Search;

internal class NoOpServerEventRouter : IServerEventRouter
{
    public Task RouteEventAsync(ServerEvent serverEvent) => Task.CompletedTask;

    public Task NotifyUserAsync(ServerEvent serverEvent, Guid userKey) => Task.CompletedTask;

    public Task BroadcastEventAsync(ServerEvent serverEvent) => Task.CompletedTask;
}
