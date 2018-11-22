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

        /// <inheritdoc/>
        public virtual void Terminate()
        { }
    }
}
