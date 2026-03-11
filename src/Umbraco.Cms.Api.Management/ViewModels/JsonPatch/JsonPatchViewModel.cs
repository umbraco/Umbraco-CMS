using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.JsonPatch;

/// <summary>
/// View model for JSON Patch operations using JsonPatch.Net.
/// </summary>
[Obsolete("Use PatchDocumentRequestModel and the custom patch engine instead. JsonPatch.Net dependency is being removed. Scheduled for removal in Umbraco 19.")]
public class JsonPatchViewModel
{
    /// <summary>
    /// Gets or sets the operation type (e.g., "replace", "add", "remove").
    /// </summary>
    [Required]
    public string Op { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON Pointer path to the target location.
    /// </summary>
    [Required]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value for the operation.
    /// </summary>
    [Required]
    public object Value { get; set; } = null!;
}
