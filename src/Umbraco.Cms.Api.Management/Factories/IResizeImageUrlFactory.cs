using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for generating URLs that resize images.
/// </summary>
public interface IResizeImageUrlFactory
{
    /// <summary>
    /// Creates URL sets for the given media items with the specified resize options.
    /// </summary>
    /// <param name="mediaItems">The media items to create URLs for.</param>
    /// <param name="options">The resize options including optional format conversion.</param>
    /// <returns>A collection of media URL info response models.</returns>
    IEnumerable<MediaUrlInfoResponseModel> CreateUrlSets(IEnumerable<IMedia> mediaItems, ImageResizeOptions options);
}
