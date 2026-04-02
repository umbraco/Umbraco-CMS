using Umbraco.Cms.Api.Management.ViewModels.Document;

namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

/// <summary>
/// Represents a response model containing information about a tracked document reference.
/// </summary>
public class DocumentReferenceResponseModel : ReferenceResponseModel
{
    /// <summary>
    /// Gets or sets a value indicating whether the document is published.
    /// </summary>
    public bool? Published { get; set; }

    /// <summary>
    /// Gets or sets the document type information for the tracked reference.
    /// </summary>
    public TrackedReferenceDocumentType DocumentType { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of variant items associated with the document reference.
    /// </summary>
    public IEnumerable<DocumentVariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<DocumentVariantItemResponseModel>();
}
