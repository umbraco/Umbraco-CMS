namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents a response model containing information about a specific version of a document in the Umbraco CMS Management API.
/// </summary>
public class DocumentVersionResponseModel : DocumentResponseModelBase<DocumentValueResponseModel, DocumentVariantResponseModel>
{
    /// <summary>
    /// Gets or sets the reference to the document.
    /// </summary>
    public ReferenceByIdModel? Document { get; set; }
}
