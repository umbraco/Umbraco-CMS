namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class ContentCreateModelBase<TValueModel, TVariantModel>
    : ContentModelBase<TValueModel, TVariantModel>
    where TValueModel : ValueModelBase
    where TVariantModel : VariantModelBase
{
    public Guid? ParentKey { get; set; }
}
