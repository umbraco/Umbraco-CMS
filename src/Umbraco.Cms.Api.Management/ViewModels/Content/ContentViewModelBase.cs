namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class ContentViewModelBase<TPropertyViewModel, TVariantViewModel>
    where TPropertyViewModel : PropertyViewModelBase
    where TVariantViewModel: VariantViewModelBase
{
    public Guid Key { get; set; }

    public Guid ContentTypeKey { get; set; }

    public IEnumerable<TPropertyViewModel> Properties { get; set; } = Array.Empty<TPropertyViewModel>();

    public IEnumerable<TVariantViewModel> Variants { get; set; } = Array.Empty<TVariantViewModel>();
}
