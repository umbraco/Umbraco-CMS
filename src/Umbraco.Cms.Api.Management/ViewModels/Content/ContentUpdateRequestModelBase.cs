namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class ContentUpdateRequestModelBase<TValueModel, TVariantModel>
    : ContentModelBase<TValueModel, TVariantModel>
    where TValueModel : ValueModelBase
    where TVariantModel : VariantModelBase
{
}
