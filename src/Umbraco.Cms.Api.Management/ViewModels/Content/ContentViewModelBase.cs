namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class ContentViewModelBase<TProperty, TVariant>
    where TProperty : PropertyViewModelBase
    where TVariant: VariantViewModelBase
{
    public Guid Key { get; set; }

    public Guid ContentTypeKey { get; set; }

    public IEnumerable<TProperty> Properties { get; set; } = Array.Empty<TProperty>();

    public IEnumerable<TVariant> Variants { get; set; } = Array.Empty<TVariant>();
}
