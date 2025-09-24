namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public class DocumentTypePropertyTypeReferenceResponseModel : ContentTypePropertyTypeReferenceResponseModel
{
    public TrackedReferenceDocumentType DocumentType { get; set; } = new();
}
