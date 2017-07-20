using LightInject;

namespace Umbraco.Core
{
    /// <summary>
    /// Defines the Umbraco runtime.
    /// </summary>
    public interface IRuntime
    {
        /// <summary>
        /// Boots the runtime.
        /// </summary>
        /// <param name="container">The application service container.</param>
        void Boot(ServiceContainer container);

        /// <summary>
        /// Terminates the runtime.
        /// </summary>
        void Terminate();
    }
}
