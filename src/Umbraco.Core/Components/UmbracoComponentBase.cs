using System;

namespace Umbraco.Core.Components
{
    /// <summary>
    /// Provides a base class for <see cref="IUmbracoComponent"/> implementations.
    /// </summary>
    public abstract class UmbracoComponentBase : IUmbracoComponent
    {
        /// <inheritdoc/>
        public virtual void Compose(Composition composition)
        { }

        public virtual Type InitializerType => null;

        /// <inheritdoc/>
        public virtual void Terminate()
        { }
    }
}
