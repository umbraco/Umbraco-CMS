namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public class DocumentReferenceResponseModel : IReferenceResponseModel
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public bool? Published { get; set; }

    public TrackedReferenceDocumentType DocumentType { get; set; } = new();
}
