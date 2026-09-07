using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Services;
using Constants = Umbraco.Cms.Search.Core.Constants;

namespace Umbraco.Cms.Search.BackOffice.Services;

/// <summary>
/// Provides backoffice child search for media.
/// </summary>
internal sealed class MediaSearchService : ContentSearchServiceBase<IMedia>, IMediaSearchService
{
    private readonly IMediaService _mediaService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaSearchService"/> class.
    /// </summary>
    /// <param name="searcher">The searcher used to query the media index.</param>
    /// <param name="mediaService">The service used to retrieve media items and their children from the database.</param>
    /// <param name="idKeyMap">The map used to resolve between media IDs and keys.</param>
    /// <param name="logger">The logger used to record warnings when a parent key cannot be resolved.</param>
    public MediaSearchService(
        ISearcher searcher,
        IMediaService mediaService,
        IIdKeyMap idKeyMap,
        ILogger<MediaSearchService> logger)
        : base(idKeyMap, searcher, logger)
        => _mediaService = mediaService;

    /// <inheritdoc />
    protected override UmbracoObjectTypes ObjectType => UmbracoObjectTypes.Media;

    /// <inheritdoc />
    protected override string IndexAlias => Umbraco.Cms.Core.Constants.IndexAliases.DraftMedia;

    /// <inheritdoc />
    protected override IEnumerable<IMedia> SearchChildrenFromDatabase(int parentId, Ordering? ordering, long pageNumber, int pageSize, out long total)
        => _mediaService.GetPagedChildren(parentId, pageNumber, pageSize, out total, null, ordering);

    /// <inheritdoc />
    protected override IEnumerable<IMedia> GetItems(IEnumerable<Guid> keys)
        => _mediaService.GetByIds(keys);
}
