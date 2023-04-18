namespace Umbraco.Cms.Core.Models;

/// <summary>
///  Represents a code snippet.
/// </summary>
public class Snippet
{
    /// <summary>
    /// The content of the snippet
    /// </summary>
    public required string Content { get; set; }

    public required string Name { get; set; }
}
