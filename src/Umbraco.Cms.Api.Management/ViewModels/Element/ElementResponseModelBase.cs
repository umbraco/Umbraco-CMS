using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Element;

public abstract class ElementResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    : ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    where TValueResponseModelBase : ValueModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    public DocumentTypeReferenceResponseModel DocumentType { get; set; } = new();
}
