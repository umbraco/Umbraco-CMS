namespace Umbraco.Cms.Core.Snippets
{
    /// <summary>
    /// Defines a partial view macro snippet.
    /// </summary>
    public interface ISnippet
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
