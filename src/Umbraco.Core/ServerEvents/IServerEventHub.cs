using Umbraco.Cms.Core.Models.ServerEvents;

namespace Umbraco.Cms.Core.ServerEvents;

public interface IServerEventHub
{
#pragma warning disable SA1300
    Task notify(ServerEvent payload);
#pragma warning restore SA1300
}
