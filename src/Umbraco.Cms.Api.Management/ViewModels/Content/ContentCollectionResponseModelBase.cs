using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.ViewModels.Content;

public abstract class ContentCollectionResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    : ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    where TValueResponseModelBase : ValueResponseModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    public string? Creator { get; set; }

    public int SortOrder { get; set; }
}
