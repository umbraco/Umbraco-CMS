namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class ContentViewModelBase<TValueViewModelBase, TVariantViewModel>
    : ContentModelBase<TValueViewModelBase, TVariantViewModel>
    where TValueViewModelBase : ValueModelBase
    where TVariantViewModel : VariantViewModelBase
{
    public Guid Key { get; set; }

    public Guid ContentTypeKey { get; set; }
}
