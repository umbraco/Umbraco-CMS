using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

internal sealed class MediaSearchService : ContentSearchServiceBase<IMedia>, IMediaSearchService
{
    private readonly IMediaService _mediaService;

    public MediaSearchService(ISqlContext sqlContext, IIdKeyMap idKeyMap, ILogger<MediaSearchService> logger, IMediaService mediaService)
        : base(sqlContext, idKeyMap, logger)
        => _mediaService = mediaService;

    protected override UmbracoObjectTypes ObjectType => UmbracoObjectTypes.Media;

    protected override Task<IEnumerable<IMedia>> SearchChildrenAsync(
        IQuery<IMedia>? query,
        int parentId,
        Ordering? ordering,
        long pageNumber,
        int pageSize,
        out long total)
        => Task.FromResult(_mediaService.GetPagedChildren(
            parentId,
            pageNumber,
            pageSize,
            out total,
            query,
            ordering));
}
