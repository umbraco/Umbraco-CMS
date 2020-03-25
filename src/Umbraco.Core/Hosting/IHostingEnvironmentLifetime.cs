namespace Umbraco.Core.Hosting
{
    public interface IHostingEnvironmentLifetime
    {
        void RegisterObject(IRegisteredObject registeredObject);
        void UnregisterObject(IRegisteredObject registeredObject);
    }
}
