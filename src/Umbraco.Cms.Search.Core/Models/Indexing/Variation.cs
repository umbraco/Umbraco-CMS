namespace Umbraco.Cms.Search.Core.Models.Indexing;

/// <summary>
/// Identifies a single culture/segment variant of a content item.
/// </summary>
/// <param name="Culture">The culture of the variant, or null for invariant.</param>
/// <param name="Segment">The segment of the variant, or null for unsegmented.</param>
public record Variation(string? Culture, string? Segment)
{
}
