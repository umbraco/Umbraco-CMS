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
        /// <param name="composition"></param>
        void Compose(Composition composition);
    }
}