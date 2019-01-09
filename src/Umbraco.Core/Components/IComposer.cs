using Umbraco.Core.Composing;

namespace Umbraco.Core.Components
{
    /// <summary>
    /// Represents a composer.
    /// </summary>
    public interface IComposer : IDiscoverable
    {
        /// <summary>
        /// Compose.
        /// </summary>
        /// <param name="composition"></param>
        void Compose(Composition composition);
    }
}