namespace Umbraco.Cms.Core.Hosting;

public interface IApplicationShutdownRegistry
{
    void RegisterObject(IRegisteredObject registeredObject);

    void UnregisterObject(IRegisteredObject registeredObject);
}
