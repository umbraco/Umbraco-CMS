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

    /// <summary>
    /// A copy of the VariationContext with a different culture.
    /// </summary>
    /// <param name="culture">The new culture.</param>
    /// <remarks>Does not modify the original VariationContext.</remarks>
    /// <returns>A copy of the VariationContext (same segment) with the culture from <paramref name="culture"/>.</returns>
    public VariationContext WithCulture(string? culture)
        => new(culture, Segment);


    /// <summary>
    /// A copy of the VariationContext with a different segment.
    /// </summary>
    /// <param name="segment">The new segment.</param>
    /// <remarks>Does not modify the original VariationContext.</remarks>
    /// <returns>A copy of the VariationContext (same culture) with the segment from <paramref name="segment"/>.</returns>
    public VariationContext WithSegment(string? segment)
        => new(Culture, segment);
}
