using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

internal sealed class ContentSearchService : ContentSearchServiceBase<IContent>, IContentSearchService
{
    private readonly IContentService _contentService;

    public ContentSearchService(ISqlContext sqlContext, IIdKeyMap idKeyMap, ILogger<ContentSearchService> logger, IContentService contentService)
        : base(sqlContext, idKeyMap, logger)
        => _contentService = contentService;

    protected override UmbracoObjectTypes ObjectType => UmbracoObjectTypes.Document;

    protected override Task<IEnumerable<IContent>> SearchChildrenAsync(
        IQuery<IContent>? query,
        int parentId,
        Ordering? ordering,
        long pageNumber,
        int pageSize,
        out long total)
        => Task.FromResult(_contentService.GetPagedChildren(
            parentId,
            pageNumber,
            pageSize,
            out total,
            query,
            ordering));
}
