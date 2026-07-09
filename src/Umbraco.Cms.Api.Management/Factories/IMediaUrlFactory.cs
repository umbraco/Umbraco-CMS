using Umbraco.Cms.Api.Management.ViewModels.Media;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Defines a factory for generating URLs for media items.
/// </summary>
public interface IMediaUrlFactory
{
    /// <summary>
    /// Creates a collection of <see cref="MediaUrlInfo"/> instances for the given media item.
    /// </summary>
    /// <param name="media">The media item to create URLs for.</param>
    /// <returns>An enumerable of <see cref="MediaUrlInfo"/> representing the URLs of the media.</returns>
    IEnumerable<MediaUrlInfo> CreateUrls(IMedia media);
    /// <summary>
    /// Generates URL sets for a collection of specified media items.
    /// </summary>
    /// <param name="mediaItems">The collection of <see cref="IMedia"/> items for which to generate URL sets.</param>
    /// <returns>An <see cref="IEnumerable{MediaUrlInfoResponseModel}"/> containing the URL sets for each media item.</returns>
    IEnumerable<MediaUrlInfoResponseModel> CreateUrlSets(IEnumerable<IMedia> mediaItems);
}
