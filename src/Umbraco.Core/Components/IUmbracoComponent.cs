using LightInject;

namespace Umbraco.Core.Components
{
    /// <summary>
    /// Represents an Umbraco component.
    /// </summary>
    public interface IUmbracoComponent
    {
        /// <summary>
        /// Composes the component.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="level">The runtime level.</param>
        void Compose(ServiceContainer container, RuntimeLevel level);

        /// <summary>
        /// Terminates the component.
        /// </summary>
        void Terminate();
    }
}
