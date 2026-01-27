namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
/// Model for operation-based partial content updates (PATCH with JSONPath).
/// </summary>
public class ContentPatchModel
{
    /// <summary>
    /// Collection of PATCH operations to apply.
    /// </summary>
    public PatchOperationModel[] Operations { get; set; } = Array.Empty<PatchOperationModel>();

    /// <summary>
    /// Cultures explicitly affected by this patch (extracted from operation paths).
    /// Used for authorization checks.
    /// </summary>
    public IEnumerable<string> AffectedCultures { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Segments explicitly affected by this patch (extracted from operation paths).
    /// </summary>
    public IEnumerable<string> AffectedSegments { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Property aliases explicitly affected by this patch (extracted from operation paths).
    /// </summary>
    public IEnumerable<string> AffectedProperties { get; set; } = Array.Empty<string>();
}
