namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Represents the variation context.
/// </summary>
public class VariationContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="VariationContext" /> class.
    /// </summary>
    public VariationContext(string? culture = null, string? segment = null)
    {
        Culture = culture ?? string.Empty; // cannot be null, default to invariant
        Segment = segment ?? string.Empty; // cannot be null, default to neutral
    }

    /// <summary>
    ///     Gets the culture.
    /// </summary>
    public string Culture { get; }

    /// <summary>
    ///     Gets the segment.
    /// </summary>
    public string Segment { get; }

    /// <summary>
    ///     Gets the segment for the content item
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public virtual string GetSegment(int contentId) => Segment;
}
