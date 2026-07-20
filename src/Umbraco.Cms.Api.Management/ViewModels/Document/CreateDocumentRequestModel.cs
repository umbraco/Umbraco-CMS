namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents the API request model used for creating a new content document in Umbraco.
/// </summary>
public class CreateDocumentRequestModel : CreateDocumentRequestModelBase<DocumentValueModel, DocumentVariantRequestModel>
{
    /// <summary>
    /// Gets or sets a reference to the template to be used for the document.
    /// </summary>
    public required ReferenceByIdModel? Template { get; set; }
}
