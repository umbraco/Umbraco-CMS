namespace Umbraco.Cms.Core.Models.Validation;

/// <summary>
///     Represents the context for property validation, including culture and segment information.
/// </summary>
public sealed class PropertyValidationContext
{
    /// <summary>
    ///     Gets the culture being validated, or null for invariant validation.
    /// </summary>
    public required string? Culture { get; init; }

    /// <summary>
    ///     Gets the segment being validated, or null for neutral segment validation.
    /// </summary>
    public required string? Segment { get; init; }

    /// <summary>
    ///     Gets the collection of cultures being validated.
    /// </summary>
    public required IEnumerable<string> CulturesBeingValidated { get; init; }

    /// <summary>
    ///     Gets the collection of segments being validated.
    /// </summary>
    public required IEnumerable<string?> SegmentsBeingValidated { get; init; }

    /// <summary>
    ///     Creates an empty property validation context with no culture or segment.
    /// </summary>
    /// <returns>An empty property validation context.</returns>
    public static PropertyValidationContext Empty() => new()
    {
        Culture = null, Segment = null, CulturesBeingValidated = [], SegmentsBeingValidated = []
    };

    /// <summary>
    ///     Creates a property validation context for a specific culture and segment.
    /// </summary>
    /// <param name="culture">The culture to validate.</param>
    /// <param name="segment">The segment to validate.</param>
    /// <returns>A property validation context for the specified culture and segment.</returns>
    public static PropertyValidationContext CultureAndSegment(string? culture, string? segment) => new()
    {
        Culture = culture, Segment = segment, CulturesBeingValidated = [], SegmentsBeingValidated = []
    };
}
