namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Request model for PATCH operations on documents.
/// </summary>
public class PatchDocumentRequestModel
{
    /// <summary>
    /// Template to apply. Null preserves existing, explicit null reference clears.
    /// </summary>
    public ReferenceByIdModel? Template { get; set; }

    /// <summary>
    /// Variants to update. Only variants present will be modified.
    /// </summary>
    public DocumentVariantPatchModel[]? Variants { get; set; }

    /// <summary>
    /// Property values to update. Only values present will be modified.
    /// </summary>
    public DocumentValuePatchModel[]? Values { get; set; }
}
