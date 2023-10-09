using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Api.Management.ViewModels.Media.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IMediaPresentationModelFactory
{
    Task<MediaResponseModel> CreateResponseModelAsync(IMedia media);
    MediaItemResponseModel CreateItemResponseModel(IMediaEntitySlim entity);
}
