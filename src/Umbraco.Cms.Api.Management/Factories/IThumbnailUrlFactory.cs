using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IThumbnailUrlFactory
{
    IEnumerable<MediaUrlInfoResponseModel> CreateUrlSets(IEnumerable<IMedia> mediaItems, int height, int width, ImageCropMode? mode);
}
