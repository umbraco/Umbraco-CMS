using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Services;
using Constants = Umbraco.Cms.Search.Core.Constants;

namespace Umbraco.Cms.Search.BackOffice.Services;

internal sealed class MediaSearchService : ContentSearchServiceBase<IMedia>, IMediaSearchService
{
    private readonly IMediaService _mediaService;

    public MediaSearchService(
        ISearcher searcher,
        IMediaService mediaService,
        IIdKeyMap idKeyMap,
        ILogger<MediaSearchService> logger)
        : base(idKeyMap, searcher, logger)
        => _mediaService = mediaService;

    protected override UmbracoObjectTypes ObjectType => UmbracoObjectTypes.Media;

    protected override string IndexAlias => Constants.IndexAliases.DraftMedia;

    protected override IEnumerable<IMedia> SearchChildrenFromDatabase(int parentId, Ordering? ordering, long pageNumber, int pageSize, out long total)
        => _mediaService.GetPagedChildren(parentId, pageNumber, pageSize, out total, null, ordering);

    protected override IEnumerable<IMedia> GetItems(IEnumerable<Guid> keys)
        => _mediaService.GetByIds(keys);
}
