using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Snippets
{
    /// <summary>
    /// Defines a partial view macro snippet.
    /// </summary>
    public interface ISnippet : IDiscoverable
    {
        /// <summary>
        /// Gets the name of the snippet.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the content of the snippet.
        /// </summary>
        string Content { get; }
    }
}
