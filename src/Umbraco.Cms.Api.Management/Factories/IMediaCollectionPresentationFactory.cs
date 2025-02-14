using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IMediaCollectionPresentationFactory
{
    Task<List<MediaCollectionResponseModel>> CreateCollectionModelAsync(ListViewPagedModel<IMedia> content);
}
