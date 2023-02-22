namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class ContentViewModelBase<TValueViewModelBase, TVariantViewModel>
    where TValueViewModelBase : ValueViewModelBase
    where TVariantViewModel : VariantViewModelBase
{
    public Guid Key { get; set; }

    public Guid ContentTypeKey { get; set; }

    public IEnumerable<TValueViewModelBase> Values { get; set; } = Array.Empty<TValueViewModelBase>();

    public IEnumerable<TVariantViewModel> Variants { get; set; } = Array.Empty<TVariantViewModel>();
}
