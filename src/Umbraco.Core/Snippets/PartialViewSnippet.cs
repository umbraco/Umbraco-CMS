namespace Umbraco.Cms.Core.Snippets;

/// <summary>
/// Defines a partial view snippet with its content.
/// </summary>
/// <remarks>
/// A partial view snippet is a predefined template that can be used when creating new partial views.
/// This class extends <see cref="PartialViewSnippetSlim"/> by including the actual content of the snippet.
/// </remarks>
public class PartialViewSnippet : PartialViewSnippetSlim
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PartialViewSnippet"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the snippet.</param>
    /// <param name="name">The display name of the snippet.</param>
    /// <param name="content">The content (Razor markup) of the snippet.</param>
    public PartialViewSnippet(string id, string name, string content)
        : base(id, name)
        => Content = content;

    /// <summary>
    /// Gets the content of the snippet.
    /// </summary>
    public string Content { get; }
}
