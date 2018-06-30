using LightInject;
using Umbraco.Core.Composing;

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
        /// <param name="concreteContainer">The application service container.</param>
        /// <param name="concreteContainer1"></param>
        void Boot(ServiceContainer concreteContainer, IContainer container);

        /// <summary>
        /// Terminates the runtime.
        /// </summary>
        void Terminate();
    }
}
