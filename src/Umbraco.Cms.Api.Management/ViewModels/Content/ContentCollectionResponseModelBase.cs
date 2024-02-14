namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class ContentCollectionResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    : ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    where TValueResponseModelBase : ValueModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    public string? Creator { get; set; }

    public int SortOrder { get; set; }
}
