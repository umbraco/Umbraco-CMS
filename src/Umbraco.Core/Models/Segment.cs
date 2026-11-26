namespace Umbraco.Cms.Core.Models;

/// <summary>
/// Represents a content segment used for personalization or A/B testing.
/// </summary>
public class Segment
{
    /// <summary>
    /// Gets or sets the display name of the segment.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the alias (identifier) of the segment.
    /// </summary>
    public required string Alias { get; set; }
}
