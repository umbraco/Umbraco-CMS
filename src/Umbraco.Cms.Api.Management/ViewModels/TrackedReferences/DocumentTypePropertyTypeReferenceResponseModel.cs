namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

/// <summary>
/// Represents the response model containing information about a property type reference within a document type.
/// Used in the API to return details about tracked references to document type property types.
/// </summary>
public class DocumentTypePropertyTypeReferenceResponseModel : ContentTypePropertyTypeReferenceResponseModel
{
    /// <summary>
    /// Gets or sets the referenced document type associated with this property type.
    /// </summary>
    public TrackedReferenceDocumentType DocumentType { get; set; } = new();
}
