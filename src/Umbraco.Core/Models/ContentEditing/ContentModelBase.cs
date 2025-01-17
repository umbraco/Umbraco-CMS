namespace Umbraco.Cms.Core.Models.ContentEditing;

public abstract class ContentModelBase<TValueModel, TVariantModel>
    where TValueModel : ValueModelBase
    where TVariantModel : VariantModelBase
{
    public IEnumerable<TValueModel> Values { get; set; } = Enumerable.Empty<TValueModel>();

    public IEnumerable<TVariantModel> Variants { get; set; } = Enumerable.Empty<TVariantModel>();
}
