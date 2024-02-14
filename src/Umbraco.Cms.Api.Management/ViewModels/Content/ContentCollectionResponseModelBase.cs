namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class ContentCollectionResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    : ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    where TValueResponseModelBase : ValueModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    public int SortOrder { get; set; }

    public string? Creator { get; set; }
}
