using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IContentCollectionPresentationFactory<TContent, TCollectionResponseModel, TValueResponseModelBase, TVariantResponseModel>
    where TContent : class, IContentBase
    where TCollectionResponseModel : ContentResponseModelBase<TValueResponseModelBase, TVariantResponseModel>
    where TValueResponseModelBase : ValueResponseModelBase
    where TVariantResponseModel : VariantResponseModelBase
{
    Task<List<TCollectionResponseModel>> CreateCollectionModelAsync(ListViewPagedModel<TContent> content);
}
