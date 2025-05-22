namespace Umbraco.Cms.Core.Models.Validation;

public sealed class PropertyValidationContext
{
    public required string? Culture { get; init; }

    public required string? Segment { get; init; }

    public required IEnumerable<string> CulturesBeingValidated { get; init; }

    public required IEnumerable<string?> SegmentsBeingValidated { get; init; }

    public static PropertyValidationContext Empty() => new()
    {
        Culture = null, Segment = null, CulturesBeingValidated = [], SegmentsBeingValidated = []
    };

    public static PropertyValidationContext CultureAndSegment(string? culture, string? segment) => new()
    {
        Culture = culture, Segment = segment, CulturesBeingValidated = [], SegmentsBeingValidated = []
    };
}
