namespace Umbraco.Cms.Core.Snippets;

/// <summary>
/// A lightweight representation of a partial view snippet (i.e. without content).
/// </summary>
/// <remarks>
/// This class is used for listing snippets without loading their full content,
/// improving performance when displaying available snippets in the UI.
/// </remarks>
public class PartialViewSnippetSlim
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PartialViewSnippetSlim"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the snippet.</param>
    /// <param name="name">The display name of the snippet.</param>
    public PartialViewSnippetSlim(string id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Gets the ID of the snippet.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the name of the snippet.
    /// </summary>
    public string Name { get; }
}
