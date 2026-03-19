using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.JsonPatch;

/// <summary>
/// Represents a view model used to encapsulate JSON Patch operations for API requests.
/// Typically contains a collection of patch operations to be applied to a resource.
/// </summary>
public class JsonPatchViewModel
{
    /// <summary>
    /// Gets or sets the type of operation to be performed in the JSON Patch document (e.g., 'add', 'remove', 'replace').
    /// </summary>
    [Required]
    public string Op { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JSON Patch path indicating the location to apply the operation.
    /// </summary>
    [Required]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value to be applied in the JSON Patch operation.
    /// </summary>
    [Required]
    public object Value { get; set; } = null!;
}
