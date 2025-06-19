using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

internal abstract class ContentSearchServiceBase<TContent> : IContentSearchService<TContent>
    where TContent : class, IContentBase
{
    private readonly ISqlContext _sqlContext;
    private readonly IIdKeyMap _idKeyMap;
    private readonly ILogger<ContentSearchServiceBase<TContent>> _logger;

    protected ContentSearchServiceBase(ISqlContext sqlContext, IIdKeyMap idKeyMap, ILogger<ContentSearchServiceBase<TContent>> logger)
    {
        _sqlContext = sqlContext;
        _idKeyMap = idKeyMap;
        _logger = logger;
    }

    protected abstract UmbracoObjectTypes ObjectType { get; }

    protected abstract Task<IEnumerable<TContent>> SearchChildrenAsync(
        IQuery<TContent>? query,
        int parentId,
        Ordering? ordering,
        long pageNumber,
        int pageSize,
        out long total);

    public async Task<PagedModel<TContent>> SearchChildrenAsync(
        string? query,
        Guid? parentId,
        Ordering? ordering,
        int skip = 0,
        int take = 100)
    {
        var parentIdAsInt = Constants.System.Root;
        if (parentId.HasValue)
        {
            Attempt<int> keyToId = _idKeyMap.GetIdForKey(parentId.Value, ObjectType);
            if (keyToId.Success is false)
            {
                _logger.LogWarning("Could not obtain an ID for parent key: {parentId} (object type: {contentType}", parentId, typeof(TContent).FullName);
                return new PagedModel<TContent>(0, []);
            }

            parentIdAsInt = keyToId.Result;
        }

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);
        IQuery<TContent>? contentQuery = ParseQuery(query);

        IEnumerable<TContent> items = await SearchChildrenAsync(contentQuery, parentIdAsInt, ordering, pageNumber, pageSize, out var total);
        return new PagedModel<TContent>
        {
            Items = items,
            Total = total,
        };
    }

    private IQuery<TContent>? ParseQuery(string? query)
    {
        // Adding multiple conditions - considering key (as Guid) & name as filter param
        Guid.TryParse(query, out Guid filterAsGuid);

        return query.IsNullOrWhiteSpace()
            ? null
            : _sqlContext
                .Query<TContent>()
                .Where(c => (c.Name != null && c.Name.Contains(query)) || c.Key == filterAsGuid);
    }
}
