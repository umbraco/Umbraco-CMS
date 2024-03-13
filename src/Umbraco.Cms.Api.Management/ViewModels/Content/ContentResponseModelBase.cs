using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    : ContentModelBase<TValueResponseModelBase, TVariantResponseModel>
    where TValueResponseModelBase : ValueModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    public Guid Id { get; set; }
}
