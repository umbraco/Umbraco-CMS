using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IReziseImageUrlFactory
{
    /// <summary>
    /// Creates URL sets for the given media items with the specified dimensions and mode.
    /// </summary>
    /// <param name="mediaItems">The media items to create URLs for.</param>
    /// <param name="height">The height of the resized image.</param>
    /// <param name="width">The width of the resized image.</param>
    /// <param name="mode">The crop mode.</param>
    /// <returns>A collection of media URL info response models.</returns>
    [Obsolete("Use the overload that accepts ImageResizeOptions instead. This method will be removed in v19.")]
    IEnumerable<MediaUrlInfoResponseModel> CreateUrlSets(IEnumerable<IMedia> mediaItems, int height, int width, ImageCropMode? mode);

    /// <summary>
    /// Creates URL sets for the given media items with the specified resize options.
    /// </summary>
    /// <param name="mediaItems">The media items to create URLs for.</param>
    /// <param name="options">The resize options including optional format conversion.</param>
    /// <returns>A collection of media URL info response models.</returns>
    IEnumerable<MediaUrlInfoResponseModel> CreateUrlSets(IEnumerable<IMedia> mediaItems, ImageResizeOptions options);
}
