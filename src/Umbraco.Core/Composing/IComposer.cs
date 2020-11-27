using Umbraco.Core.Builder;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Represents a composer.
    /// </summary>
    public interface IComposer : IDiscoverable
    {
        /// <summary>
        /// Compose.
        /// </summary>
        void Compose(IUmbracoBuilder builder);
    }
}
