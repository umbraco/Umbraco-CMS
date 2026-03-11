namespace Umbraco.Cms.Api.Management.ViewModels.Patching;

/// <summary>
/// Model for operation-based partial content updates using Umbraco's extended JSON Pointer path syntax.
/// </summary>
public class ContentPatchModel
{
    /// <summary>
    /// Gets or sets collection of PATCH operations to apply.
    /// </summary>
    public PatchOperationModel[] Operations { get; set; } = Array.Empty<PatchOperationModel>();

    /// <summary>
    /// Gets or sets cultures explicitly affected by this patch (extracted from operation paths).
    /// Used for authorization checks.
    /// </summary>
    public IEnumerable<string> AffectedCultures { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets segments explicitly affected by this patch (extracted from operation paths).
    /// </summary>
    public IEnumerable<string> AffectedSegments { get; set; } = Array.Empty<string>();

}
