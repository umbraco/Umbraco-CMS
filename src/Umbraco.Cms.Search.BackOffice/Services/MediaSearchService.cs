using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
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
    private readonly IIdKeyMap _idKeyMap;
    private readonly ILogger<MediaSearchService> _logger;

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
        : base(searcher, logger)
    {
        _mediaService = mediaService;
        _idKeyMap = idKeyMap;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override UmbracoObjectTypes ObjectType => UmbracoObjectTypes.Media;

    /// <inheritdoc />
    protected override string IndexAlias => Umbraco.Cms.Core.Constants.IndexAliases.DraftMedia;

    /// <inheritdoc />
    protected override async Task<PagedModel<IMedia>> SearchChildrenFromDatabaseAsync(Guid? parentId, Ordering? ordering, int skip, int take)
    {
        var parentIdAsInt = Umbraco.Cms.Core.Constants.System.Root;
        if (parentId.HasValue)
        {
            Attempt<int> keyToId = await _idKeyMap.GetIdForKeyAsync(parentId.Value, UmbracoObjectTypes.Media);
            if (keyToId.Success is false)
            {
                _logger.LogWarning("Could not obtain an ID for parent key: {parentId} (object type: Media)", parentId);
                return new PagedModel<IMedia>(0, []);
            }

            parentIdAsInt = keyToId.Result;
        }

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        IEnumerable<IMedia> items = _mediaService.GetPagedChildren(parentIdAsInt, pageNumber, pageSize, out var total, null, ordering);
        return new PagedModel<IMedia> { Items = items, Total = total };
    }

    /// <inheritdoc />
    protected override IEnumerable<IMedia> GetItems(IEnumerable<Guid> keys)
        => _mediaService.GetByIds(keys);
}
