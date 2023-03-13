namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class ContentModelBase<TValueModel, TVariantModel>
    where TValueModel : ValueModelBase
    where TVariantModel : VariantModelBase
{
    public IEnumerable<TValueModel> Values { get; set; } = Array.Empty<TValueModel>();

    public IEnumerable<TVariantModel> Variants { get; set; } = Array.Empty<TVariantModel>();
}
