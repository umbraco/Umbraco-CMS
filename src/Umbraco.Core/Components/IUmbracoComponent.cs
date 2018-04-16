using Umbraco.Core.Composing;

namespace Umbraco.Core.Components
{
    /// <summary>
    /// Represents an Umbraco component.
    /// </summary>
    public interface IUmbracoComponent : IDiscoverable
    {
        /// <summary>
        /// Composes the component.
        /// </summary>
        /// <param name="composition">The composition.</param>
        void Compose(Composition composition);

        /// <summary>
        /// Terminates the component.
        /// </summary>
        void Terminate();
    }
}
