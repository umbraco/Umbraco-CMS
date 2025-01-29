namespace Umbraco.Cms.Core.Models.Context;

/// <summary>
///     Represents the back-office variation context.
/// </summary>
public class BackOfficeVariationContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BackOfficeVariationContext" /> class.
    /// </summary>
    public BackOfficeVariationContext(string? culture = null, string? segment = null)
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
}
