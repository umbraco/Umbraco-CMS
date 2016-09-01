using LightInject;

namespace Umbraco.Core.Components
{
    /// <summary>
    /// Provides a base class for <see cref="IUmbracoComponent"/> implementations.
    /// </summary>
    public abstract class UmbracoComponentBase : IUmbracoComponent
    {
        /// <inheritdoc/>
        public virtual void Compose(ServiceContainer container, RuntimeLevel level)
        {
            Compose(container);
        }

        /// <summary>
        /// Composes the component.
        /// </summary>
        /// <param name="container">The container.</param>
        public virtual void Compose(ServiceContainer container)
        { }

        /// <inheritdoc/>
        public virtual void Terminate()
        { }
    }
}
