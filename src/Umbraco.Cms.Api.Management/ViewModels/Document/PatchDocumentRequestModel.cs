using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Request model for operation-based PATCH on documents using JSON Patch with JSONPath.
/// </summary>
public class PatchDocumentRequestModel
{
    /// <summary>
    /// Collection of PATCH operations to apply to the document.
    /// Operations are applied sequentially and atomically (all-or-nothing).
    /// </summary>
    [Required]
    [MinLength(1)]
    public PatchOperationRequestModel[] Operations { get; set; } = Array.Empty<PatchOperationRequestModel>();
}
