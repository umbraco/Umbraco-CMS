namespace Umbraco.Cms.Api.Management.ViewModels.Media;

/// <summary>
/// Represents a response model that provides information about a media item's URL.
/// </summary>
public sealed class MediaUrlInfoResponseModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.ViewModels.Media.MediaUrlInfoResponseModel"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the media item.</param>
    /// <param name="urlInfos">The collection of URL information associated with the media item.</param>
    public MediaUrlInfoResponseModel(Guid id, IEnumerable<MediaUrlInfo> urlInfos)
    {
        Id = id;
        UrlInfos = urlInfos;
    }

    /// <summary>
    /// Gets the unique identifier of the media item associated with this URL info.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the collection of URL information associated with the media item.
    /// </summary>
    public IEnumerable<MediaUrlInfo> UrlInfos { get; }
}
