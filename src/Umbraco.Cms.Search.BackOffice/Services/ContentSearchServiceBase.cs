using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Search.BackOffice.Services;

/// <summary>
/// Provides backoffice child search for a content type, falling back to the database when no query is given.
/// </summary>
/// <typeparam name="TContent">The type of content item to search for.</typeparam>
internal abstract class ContentSearchServiceBase<TContent> : IndexedSearchServiceBase, IContentSearchService<TContent>
    where TContent : class, IContentBase
{
    private readonly IIdKeyMap _idKeyMap;
    private readonly ISearcher _searcher;
    private readonly ILogger<ContentSearchServiceBase<TContent>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentSearchServiceBase{TContent}"/> class.
    /// </summary>
    /// <param name="idKeyMap">The map used to resolve between numeric IDs and keys.</param>
    /// <param name="searcher">The searcher used to query the search index.</param>
    /// <param name="logger">The logger used to record warnings when a parent key cannot be resolved.</param>
    protected ContentSearchServiceBase(IIdKeyMap idKeyMap, ISearcher searcher, ILogger<ContentSearchServiceBase<TContent>> logger)
    {
        _idKeyMap = idKeyMap;
        _searcher = searcher;
        _logger = logger;
    }

    /// <summary>
    /// Gets the entity type this service searches, used to resolve start nodes and parent keys.
    /// </summary>
    protected abstract UmbracoObjectTypes ObjectType { get; }

    /// <summary>
    /// Gets the alias of the search index to query.
    /// </summary>
    protected abstract string IndexAlias { get; }

    /// <summary>
    /// Retrieves a page of children directly from the database, used when there is no search query.
    /// </summary>
    /// <param name="parentId">The numeric ID of the parent, or the root ID for top-level content.</param>
    /// <param name="ordering">The ordering to apply.</param>
    /// <param name="pageNumber">The zero-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="total">The total number of children, regardless of paging.</param>
    /// <returns>The requested page of children.</returns>
    protected abstract IEnumerable<TContent> SearchChildrenFromDatabase(
        int parentId,
        Ordering? ordering,
        long pageNumber,
        int pageSize,
        out long total);

    /// <summary>
    /// Retrieves content items by key, used to hydrate search results with full items.
    /// </summary>
    /// <param name="keys">The keys of the items to retrieve.</param>
    /// <returns>The matching items, in no guaranteed order.</returns>
    protected abstract IEnumerable<TContent> GetItems(IEnumerable<Guid> keys);

    /// <summary>
    /// Searches for children via the search index and hydrates the matching items, preserving result order.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <param name="parentId">The parent content item key, or null to search root-level content.</param>
    /// <param name="ordering">The ordering to apply.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <returns>A paged model of content items.</returns>
    protected async Task<PagedModel<TContent>> SearchChildrenFromIndexAsync(
        string? query,
        Guid? parentId,
        Ordering? ordering,
        int skip,
        int take)
    {
        List<Filter> filters = ParseFilters(query, parentId, out var effectiveQuery);

        // this method only searches for children, not descendants; if there is no parent ID, explicitly match root level content
        if (parentId.HasValue is false)
        {
            filters.Add(new IntegerExactFilter(Core.Constants.FieldNames.Level, [1], false));
        }

        Sorter sorter = GetSorter(ordering);

        SearchResult result = await _searcher.SearchAsync(
            IndexAlias,
            query: effectiveQuery,
            filters: filters,
            facets: null,
            sorters: [sorter],
            culture: ordering?.Culture,
            segment: null,
            accessContext: null,
            skip,
            take);

        Guid[] resultKeys = result.Documents.Select(d => d.Id).ToArray();
        TContent[] resultItems = resultKeys.Length > 0
            ? GetItems(resultKeys)
                // unfortunately we can't explicitly rely on the underlying services ordering the requested
                // items correctly, so we need to enforce correct ordering here.
                .OrderBy(item => resultKeys.IndexOf(item.Key))
                .ToArray()
            : [];

        return new PagedModel<TContent> { Items = resultItems, Total = result.Total };
    }

    /// <inheritdoc />
    public async Task<PagedModel<TContent>> SearchChildrenAsync(
        string? query,
        Guid? parentId,
        Ordering? ordering,
        int skip = 0,
        int take = 100)
    {
        if (query.IsNullOrWhiteSpace())
        {
            return SearchChildrenFromDatabase(parentId, ordering, skip, take);
        }

        return await SearchChildrenFromIndexAsync(query, parentId, ordering, skip, take);
    }

    /// <summary>
    /// Retrieves a page of children directly from the database, resolving <paramref name="parentId"/> to its numeric ID.
    /// </summary>
    /// <param name="parentId">The parent content item key, or null to search root-level content.</param>
    /// <param name="ordering">The ordering to apply.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <returns>A paged model of content items, empty if <paramref name="parentId"/> could not be resolved.</returns>
    private PagedModel<TContent> SearchChildrenFromDatabase(Guid? parentId, Ordering? ordering, int skip, int take)
    {
        var parentIdAsInt = Constants.System.Root;
        if (parentId.HasValue)
        {
            Attempt<int> keyToId = _idKeyMap.GetIdForKey(parentId.Value, ObjectType);
            if (keyToId.Success is false)
            {
                _logger.LogWarning("Could not obtain an ID for parent key: {parentKey} (object type: {contentType})", parentId, typeof(TContent).FullName);
                return new PagedModel<TContent>(0, []);
            }

            parentIdAsInt = keyToId.Result;
        }

        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        IEnumerable<TContent> items = SearchChildrenFromDatabase(parentIdAsInt, ordering, pageNumber, pageSize, out var total);
        return new PagedModel<TContent>
        {
            Items = items,
            Total = total,
        };
    }

    /// <summary>
    /// Maps an <see cref="Ordering"/> to an index sorter, falling back to the default sorter for unsupported or unset fields.
    /// </summary>
    /// <param name="ordering">The requested ordering.</param>
    /// <returns>The sorter to use.</returns>
    private Sorter GetSorter(Ordering? ordering)
    {
        if (ordering?.OrderBy is null)
        {
            return Sorting.Default();
        }

        if (ordering.IsCustomField)
        {
            // TODO: support custom field ordering
            return Sorting.Default();
        }

        switch (ordering.OrderBy)
        {
            case "name":
                return new TextSorter(Core.Constants.FieldNames.Name, ordering.Direction);
            case "updateDate":
                return new DateTimeOffsetSorter(Core.Constants.FieldNames.UpdateDate, ordering.Direction);
            case "creator":
            case "owner":
                // NOTE: "creator" / "owner" is configurable for list view but not supported here,
                //       because this will require a full re-index when any username is changed
                _logger.LogInformation("The system field \"{field}\" does not support sorting by indexed content search.", ordering.OrderBy);
                return Sorting.Default();
            default:
                _logger.LogInformation("The system field \"{field}\" could not be converted into a sorting by indexed content search.", ordering.OrderBy);
                return Sorting.Default();
        }
    }
}
