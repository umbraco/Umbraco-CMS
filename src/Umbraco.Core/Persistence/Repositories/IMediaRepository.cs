using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IMedia" /> entities.
/// </summary>
public interface IMediaRepository : IContentRepository<int, IMedia>, IReadRepository<Guid, IMedia>
{
    /// <summary>
    ///     Gets a media item by its file path.
    /// </summary>
    /// <param name="mediaPath">The file path of the media item.</param>
    /// <returns>The media item if found; otherwise, <c>null</c>.</returns>
    IMedia? GetMediaByPath(string mediaPath);

    /// <summary>
    ///     Returns true if there is any media in the recycle bin.
    /// </summary>
    /// <returns><c>true</c> if the recycle bin contains items; otherwise, <c>false</c>.</returns>
    bool RecycleBinSmells();
}
