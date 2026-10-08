using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Search.Querying;
using Umbraco.Cms.Core.Search.Querying.Filtering;
using Umbraco.Cms.Core.Search.Querying.Sorting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Base class for backoffice child search over indexed content, falling back to the database when no query is given.
/// </summary>
/// <typeparam name="TContent">The content type searched for.</typeparam>
public abstract class ContentSearchServiceBase<TContent> : IndexedSearchServiceBase, IContentSearchService<TContent>
    where TContent : class, IContentBase
{
    private readonly ISearcherResolver _searcherResolver;
    private readonly ILogger<ContentSearchServiceBase<TContent>> _logger;

    protected ContentSearchServiceBase(ISearcherResolver searcherResolver, ILogger<ContentSearchServiceBase<TContent>> logger)
    {
        _searcherResolver = searcherResolver;
        _logger = logger;
    }

    protected abstract UmbracoObjectTypes ObjectType { get; }

    protected abstract string IndexAlias { get; }

    /// <summary>
    /// Gets a page of children straight from the database, for searches without a query.
    /// </summary>
    protected abstract Task<PagedModel<TContent>> SearchChildrenFromDatabaseAsync(
        Guid? parentId,
        string[]? propertyAliases,
        Ordering? ordering,
        bool loadTemplates,
        int skip,
        int take);

    /// <summary>
    /// Gets the items the index returned, loading only the requested properties and, optionally, templates.
    /// </summary>
    protected abstract Task<IEnumerable<TContent>> GetItemsAsync(
        IEnumerable<Guid> keys,
        string[]? propertyAliases,
        bool loadTemplates,
        CancellationToken cancellationToken);

    protected async Task<PagedModel<TContent>> SearchChildrenFromIndexAsync(
        string? query,
        Guid? parentId,
        string[]? propertyAliases,
        Ordering? ordering,
        bool loadTemplates,
        int skip,
        int take)
    {
        List<Filter> filters = ParseFilters(query, parentId, out var effectiveQuery);

        // this method only searches for children, not descendants; if there is no parent ID, explicitly match root level content
        if (parentId.HasValue is false)
        {
            filters.Add(new IntegerExactFilter(Constants.Search.FieldNames.Level, [1], false));
        }

        Sorter sorter = GetSorter(ordering);

        ISearcher searcher = _searcherResolver.GetRequiredSearcher(IndexAlias);
        SearchResult result = await searcher.SearchAsync(
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
            ? (await GetItemsAsync(resultKeys, propertyAliases, loadTemplates, CancellationToken.None))
                // unfortunately we can't explicitly rely on the underlying services ordering the requested
                // items correctly, so we need to enforce correct ordering here.
                .OrderBy(item => resultKeys.IndexOf(item.Key))
                .ToArray()
            : [];

        return new PagedModel<TContent> { Items = resultItems, Total = result.Total };
    }

    public async Task<PagedModel<TContent>> SearchChildrenAsync(
        string? query,
        Guid? parentId,
        string[]? propertyAliases,
        Ordering? ordering,
        bool loadTemplates = true,
        int skip = 0,
        int take = 100)
    {
        if (query.IsNullOrWhiteSpace())
        {
            return await SearchChildrenFromDatabaseAsync(parentId, propertyAliases, ordering, loadTemplates, skip, take);
        }

        return await SearchChildrenFromIndexAsync(query, parentId, propertyAliases, ordering, loadTemplates, skip, take);
    }

    private Sorter GetSorter(Ordering? ordering)
    {
        if (ordering?.OrderBy is null)
        {
            return DefaultSorter();
        }

        if (ordering.IsCustomField)
        {
            // TODO: support custom field ordering
            return DefaultSorter();
        }

        switch (ordering.OrderBy)
        {
            case "name":
                return new TextSorter(Constants.Search.FieldNames.Name, ordering.Direction);
            case "updateDate":
                return new DateTimeOffsetSorter(Constants.Search.FieldNames.UpdateDate, ordering.Direction);
            case "creator":
            case "owner":
                // NOTE: "creator" / "owner" is configurable for list view but not supported here,
                //       because this will require a full re-index when any username is changed
                _logger.LogInformation("The system field \"{field}\" does not support sorting by indexed content search.", ordering.OrderBy);
                return DefaultSorter();
            default:
                _logger.LogInformation("The system field \"{field}\" could not be converted into a sorting by indexed content search.", ordering.OrderBy);
                return DefaultSorter();
        }
    }
}
