using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Document;

public abstract class DocumentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    : ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    where TValueResponseModelBase : ValueModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    public DocumentTypeReferenceResponseModel DocumentType { get; set; } = new();
}
