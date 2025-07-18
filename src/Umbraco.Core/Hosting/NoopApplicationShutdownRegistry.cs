namespace Umbraco.Cms.Core.Hosting;

internal sealed class NoopApplicationShutdownRegistry : IApplicationShutdownRegistry
{
    public void RegisterObject(IRegisteredObject registeredObject)
    {
    }

    public void UnregisterObject(IRegisteredObject registeredObject)
    {
    }
}
