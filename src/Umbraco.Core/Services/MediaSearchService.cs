using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides backoffice child search for media.
/// </summary>
public sealed class MediaSearchService : ContentSearchServiceBase<IMedia>, IMediaSearchService
{
    private readonly IMediaService _mediaService;
    private readonly IIdKeyMap _idKeyMap;
    private readonly ILogger<MediaSearchService> _logger;

    public MediaSearchService(
        ISearcherResolver searcherResolver,
        IMediaService mediaService,
        IIdKeyMap idKeyMap,
        ILogger<MediaSearchService> logger)
        : base(searcherResolver, logger)
    {
        _mediaService = mediaService;
        _idKeyMap = idKeyMap;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override UmbracoObjectTypes ObjectType => UmbracoObjectTypes.Media;

    /// <inheritdoc />
    protected override string IndexAlias => Constants.Search.IndexAliases.DraftMedia;

    /// <inheritdoc />
    protected override async Task<PagedModel<IMedia>> SearchChildrenFromDatabaseAsync(Guid? parentId, string[]? propertyAliases, Ordering? ordering, bool loadTemplates, int skip, int take)
    {
        var parentIdAsInt = Constants.System.Root;
        if (parentId.HasValue)
        {
            Attempt<int> keyToId = await _idKeyMap.GetIdForKeyAsync(parentId.Value, UmbracoObjectTypes.Media);
            if (keyToId.Success is false)
            {
                _logger.LogWarning("Could not obtain an ID for parent key: {parentKey} (object type: Media)", parentId);
                return new PagedModel<IMedia>(0, []);
            }

            parentIdAsInt = keyToId.Result;
        }

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        IEnumerable<IMedia> items = _mediaService.GetPagedChildren(parentIdAsInt, pageNumber, pageSize, out var total, propertyAliases, filter: null, ordering);
        return new PagedModel<IMedia> { Items = items, Total = total };
    }

    /// <inheritdoc />
    protected override Task<IEnumerable<IMedia>> GetItemsAsync(IEnumerable<Guid> keys, string[]? propertyAliases, bool loadTemplates, CancellationToken cancellationToken)
        => Task.FromResult(_mediaService.GetByIds(keys, propertyAliases));
}
