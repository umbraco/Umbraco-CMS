using Umbraco.Cms.Api.Management.ViewModels.Document;

namespace Umbraco.Cms.Api.Management.ViewModels.TrackedReferences;

public class DocumentReferenceResponseModel : ReferenceResponseModel
{
    public bool? Published { get; set; }

    public TrackedReferenceDocumentType DocumentType { get; set; } = new();

    public IEnumerable<DocumentVariantItemResponseModel> Variants { get; set; } = Enumerable.Empty<DocumentVariantItemResponseModel>();
}
