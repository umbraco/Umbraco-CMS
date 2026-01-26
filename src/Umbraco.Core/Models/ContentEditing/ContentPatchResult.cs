namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
/// Result of a content patch operation.
/// </summary>
public class ContentPatchResult
{
    public required IContent Content { get; init; }

    /// <summary>
    /// Cultures that were modified by this patch.
    /// </summary>
    public IEnumerable<string> AffectedCultures { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Property aliases that were modified by this patch.
    /// </summary>
    public IEnumerable<string> AffectedProperties { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Validation result for properties.
    /// </summary>
    public ContentValidationResult ValidationResult { get; init; } = new();
}
