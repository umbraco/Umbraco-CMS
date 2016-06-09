using LightInject;

namespace Umbraco.Core
{
    // must remain internal - this class is here to support the transition from singletons
    // and resolvers to injection - by providing a static access to singleton services - it
    // is initialized once with a service container, in WebBootManager.
    internal class Current
    {
        public static IServiceContainer Container { get; set; } // ok to set - don't be stupid
    }
}
