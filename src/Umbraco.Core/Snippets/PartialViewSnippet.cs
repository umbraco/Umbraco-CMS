namespace Umbraco.Cms.Core.Snippets;

/// <summary>
/// Defines a partial view snippet.
/// </summary>
public class PartialViewSnippet : PartialViewSnippetSlim
{
    public PartialViewSnippet(string id, string name, string content)
        : base(id, name)
        => Content = content;

    /// <summary>
    /// Gets the content of the snippet.
    /// </summary>
    public string Content { get; }
}
