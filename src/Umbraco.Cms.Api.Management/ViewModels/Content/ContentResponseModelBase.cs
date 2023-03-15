namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    : ContentModelBase<TValueResponseModelBase, TVariantResponseModel>
    where TValueResponseModelBase : ValueModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    public Guid Key { get; set; }

    public Guid ContentTypeKey { get; set; }
}
