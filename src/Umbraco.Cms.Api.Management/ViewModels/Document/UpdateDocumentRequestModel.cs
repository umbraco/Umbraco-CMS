namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents a request model used for updating a document via the API.
/// </summary>
public class UpdateDocumentRequestModel : UpdateDocumentRequestModelBase<DocumentValueModel, DocumentVariantRequestModel>
{
    /// <summary>
    /// Gets or sets the template associated with the document, referenced by its ID.
    /// </summary>
    public ReferenceByIdModel? Template { get; set; }
}
