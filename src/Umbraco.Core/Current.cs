using LightInject;

namespace Umbraco.Core
{
    // kill this eventually - it's here during the transition
    // provides static access to singletons
    internal class Current
    {
        public static IServiceContainer Container { get; set; } // ok to set - don't be stupid
    }
}
