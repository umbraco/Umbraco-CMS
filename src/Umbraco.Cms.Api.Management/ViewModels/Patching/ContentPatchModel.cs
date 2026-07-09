namespace Umbraco.Cms.Api.Management.ViewModels.Patching;

/// <summary>
/// Model for operation-based partial content updates using Umbraco's extended JSON Pointer path syntax.
/// </summary>
public class ContentPatchModel
{
    /// <summary>
    /// Gets or sets collection of PATCH operations to apply.
    /// </summary>
    public PatchOperationModel[] Operations { get; set; } = [];
}
