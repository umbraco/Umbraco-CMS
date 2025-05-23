namespace Umbraco.Cms.Core.Snippets;

/// <summary>
/// A lightweight representation of a partial view snippet (i.e. without content).
/// </summary>
public class PartialViewSnippetSlim
{
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
