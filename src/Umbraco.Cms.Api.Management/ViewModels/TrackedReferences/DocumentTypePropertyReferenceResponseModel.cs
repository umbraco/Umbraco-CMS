namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public class DocumentTypePropertyReferenceResponseModel : ContentTypePropertyReferenceResponseModel
{
    public TrackedReferenceDocumentType DocumentType { get; set; } = new();
}
