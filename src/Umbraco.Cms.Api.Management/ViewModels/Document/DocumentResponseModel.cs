namespace Umbraco.Cms.Api.Management.ViewModels.Document;

/// <summary>
/// Represents a response model for a document returned by the Umbraco CMS Management API.
/// </summary>
public class DocumentResponseModel : DocumentResponseModelBase<DocumentValueResponseModel, DocumentVariantResponseModel>
{
    /// <summary>Gets or sets the template reference associated with the document.</summary>
    public ReferenceByIdModel? Template { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the document is trashed.
    /// </summary>
    public bool IsTrashed { get; set; }
}
