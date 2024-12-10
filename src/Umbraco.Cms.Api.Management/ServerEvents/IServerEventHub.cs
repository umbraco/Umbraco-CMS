using Umbraco.Cms.Api.Management.ServerEvents.Models;

namespace Umbraco.Cms.Api.Management.ServerEvents;

public interface IServerEventHub
{
#pragma warning disable SA1300
    Task notify(ServerEvent payload);
#pragma warning restore SA1300
}
