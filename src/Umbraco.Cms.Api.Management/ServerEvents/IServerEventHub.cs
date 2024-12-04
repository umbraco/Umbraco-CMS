namespace Umbraco.Cms.Api.Management.Routing;

public interface IServerEventHub
{
#pragma warning disable SA1300
    Task notify();
#pragma warning restore SA1300
}
