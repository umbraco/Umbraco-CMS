using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IMediaPresentationModelFactory
{
    Task<MediaResponseModel> CreateResponseModelAsync(IMedia media);
}
