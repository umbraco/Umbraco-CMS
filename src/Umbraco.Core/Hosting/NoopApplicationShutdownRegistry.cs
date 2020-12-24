namespace Umbraco.Core.Hosting
{
    internal class NoopApplicationShutdownRegistry : IApplicationShutdownRegistry
    {
        public void RegisterObject(IRegisteredObject registeredObject) { }
        public void UnregisterObject(IRegisteredObject registeredObject) { }
    }
}
