using System.Collections.Concurrent;
using System.Reflection;
using Examine;
using Examine.Lucene;
using Examine.Search;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Faceting;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;
using Umbraco.Cms.Search.Provider.Examine.Configuration;
using Umbraco.Cms.Search.Provider.Examine.Extensions;
using Umbraco.Cms.Search.Provider.Examine.Helpers;
using Umbraco.Cms.Search.Provider.Examine.Models.Searching.Filtering;
using Umbraco.Extensions;
using FacetResult = Umbraco.Cms.Search.Core.Models.Searching.Faceting.FacetResult;
using SearchResult = Umbraco.Cms.Search.Core.Models.Searching.SearchResult;

namespace Umbraco.Cms.Search.Provider.Examine.Services;

public class Searcher : IExamineSearcher
{
    private readonly IActiveIndexManager _activeIndexManager;
    private static readonly ConcurrentDictionary<string, bool> InitializedIndexes = new();

    public Searcher(IExamineManager examineManager, IOptions<SearcherOptions> searcherOptions, IActiveIndexManager activeIndexManager)
    {
        _activeIndexManager = activeIndexManager;
        ExamineManager = examineManager;
        SearcherOptions = searcherOptions.Value;
    }

    /// <summary>
    /// Gets the Examine manager for use in derived classes.
    /// </summary>
    protected IExamineManager ExamineManager { get; }

    /// <summary>
    /// Gets the searcher options configuration for use in derived classes.
    /// </summary>
    protected SearcherOptions SearcherOptions { get; }

    public async Task<SearchResult> SearchAsync(
        string indexAlias,
        string? query,
        IEnumerable<Filter>? filters,
        IEnumerable<Facet>? facets,
        IEnumerable<Sorter>? sorters,
        string? culture,
        string? segment,
        AccessContext? accessContext,
        int skip,
        int take,
        int maxSuggestions = 0)
    {
        // Special case if no parameters are provided, return an empty list.
        if (query is null && filters is null && facets is null && sorters is null && culture is null && segment is null && accessContext is null)
        {
            return new SearchResult(0, Array.Empty<Document>(), Array.Empty<FacetResult>());
        }

        var physicalIndexName = _activeIndexManager.ResolveActiveIndexName(indexAlias);
        if (ExamineManager.TryGetIndex(physicalIndexName, out IIndex? index) is false)
        {
            return new SearchResult(0, Array.Empty<Document>(), Array.Empty<FacetResult>());
        }

        EnsureFieldAnalyzersLoaded(index);
        SearchResult? searchResult;

        if (SearcherOptions.ExpandFacetValues)
        {
            Filter[]? filtersAsArray = filters as Filter[] ?? filters?.ToArray();
            Facet[]? facetsAsArray = facets as Facet[] ?? facets?.ToArray();
            Facet[] filterFacets = filtersAsArray is not null && facetsAsArray is not null
                ? facetsAsArray.Where(facet => filtersAsArray.Any(filter => filter.FieldName == facet.FieldName)).ToArray()
                : [];

            var facetFilterResults = new List<FacetResult>();
            foreach (Facet facet in filterFacets)
            {
                IBooleanOperation facetSearchQuery = CreateBaseQuery();
                Filter[] effectiveFilters = filtersAsArray!.Where(filter => filter.FieldName != facet.FieldName).ToArray();
                facetFilterResults.AddRange(Search(facetSearchQuery, effectiveFilters, [facet], null, culture, segment, 0, 0).Facets);
            }
            SearchResult documentsSearchResult = Search(CreateBaseQuery(), filtersAsArray, facetsAsArray?.Except(filterFacets), sorters, culture, segment, skip, take);
            searchResult = documentsSearchResult with
            {
                Facets = facetFilterResults.Union(documentsSearchResult.Facets)
            };
        }
        else
        {
            searchResult = Search(CreateBaseQuery(), filters, facets, sorters, culture, segment, skip, take);
        }

        if (maxSuggestions > 0)
        {
            IEnumerable<string> suggestions = await GetSuggestionsAsync(indexAlias, query, culture, segment, maxSuggestions);
            return searchResult with { Suggestions = suggestions };
        }

        return searchResult;

        IBooleanOperation CreateBaseQuery()
        {
            IBooleanOperation searchQuery = index.Searcher
                .CreateQuery()
                .GroupedOr([Constants.SystemFields.Culture], culture is null ? [Constants.Variance.Invariant] : [culture, Constants.Variance.Invariant]);

            if (query is not null)
            {
                // when performing wildcard search on phrase queries, the results can be somewhat surprising. for example:
                // 1. wildcard search for "something whatever" yields all documents with either "something" or "whatever".
                // 2. wildcard searching for "some what" does not yield documents with "something", but it does yield
                //    documents with "whatever" because the wildcard is applied at the end of the query.
                // to counter for these cases, we split the query into multiple terms and apply wildcard search to each
                // term with AND grouping.
                var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var term in terms)
                {
                    searchQuery.And().Group(nestedQuery => CreateAggregatedTextQuery(nestedQuery, term, segment));
                }
            }

            AddProtection(searchQuery, accessContext);

            return searchQuery;
        }
    }

    private SearchResult Search(
        IBooleanOperation searchQuery,
        IEnumerable<Filter>? filters,
        IEnumerable<Facet>? facets,
        IEnumerable<Sorter>? sorters,
        string? culture,
        string? segment,
        int skip,
        int take)
    {
        // Examine will overwrite subsequent facets of the same field, so make sure there aren't any duplicates.
        Facet[] deduplicateFacets = DeduplicateFacets(facets);

        Sorter[]? sortersAsArray = sorters as Sorter[] ?? sorters?.ToArray();

        // Add facets and filters
        AddFilters(searchQuery, filters, culture, segment);
        AddFacets(searchQuery, deduplicateFacets, culture, segment);
        AddSorters(searchQuery, sortersAsArray, culture, segment);

        // We only need the IndexType and NodeId
        var selectedFields = new HashSet<string> { ExamineFieldNames.CategoryFieldName, ExamineFieldNames.ItemIdFieldName };
        searchQuery.SelectFields(selectedFields);

        ISearchResults results;
        try
        {
            results = searchQuery.Execute(new QueryOptions(skip, take));
        }
        catch (ArgumentException e)
        {
            if (e.Message.Contains("field \"$facets\" was not indexed with SortedSetDocValues"))
            {
                throw new ConfigurationException("Tried querying a facet that did not exist, please configure your facets with FieldOptions.", e);
            }

            if (e.Message.Contains("dimension \"") && e.Message.Contains("\" was not indexed"))
            {
                throw new ConfigurationException("Tried querying a facet that did not exist, but the field did exist, please configure your facets with FieldOptions and set Facetable to true.", e);
            }

            throw;
        }

        IEnumerable<ISearchResult> searchResults = results.ToArray();

        FacetResult[] facetResults = facets is null ? [] : MapFacets(results, deduplicateFacets, culture, segment).ToArray();
        Document[] searchResultDocuments = take > 0 ? searchResults.Select(MapToDocument).WhereNotNull().ToArray() : [];

        return new SearchResult(results.TotalItemCount, searchResultDocuments, facetResults);
    }

    private void AddProtection(IBooleanOperation searchQuery, AccessContext? accessContext)
    {
        if (accessContext?.Bypass is true)
        {
            return;
        }

        if (accessContext is null || accessContext.PrincipalId == Guid.Empty)
        {
            searchQuery.And().Field(Constants.SystemFields.Protection, Guid.Empty.AsKeyword());
        }
        else
        {
            List<string> keys = [Guid.Empty.AsKeyword(), accessContext.PrincipalId.AsKeyword()];

            if (accessContext.GroupIds is not null)
            {
                keys.AddRange(accessContext.GroupIds.Select(groupId => groupId.AsKeyword()));
            }

            searchQuery.And().GroupedOr([Constants.SystemFields.Protection], keys.ToArray());
        }
    }

    private void AddSorters(IBooleanOperation searchQuery, IEnumerable<Sorter>? sorters, string? culture, string? segment)
    {
        if (sorters is null)
        {
            return;
        }

        // TODO: Handling of multiple sorters, does this hold up?
        foreach (Sorter sorter in sorters)
        {
            SortableField[]? sortableFields = MapSorter(sorter);
            if (sortableFields is null)
            {
                // Custom sorter handling - let derived class manipulate the query directly
                AddCustomSorter(searchQuery, sorter, culture, segment);
                continue;
            }

            if (sortableFields.Length == 0)
            {
                continue;
            }

            if (sorter.Direction is Direction.Ascending)
            {
                searchQuery.OrderBy(sortableFields);
            }
            else
            {
                searchQuery.OrderByDescending(sortableFields);
            }
        }
    }

    private SortableField[]? MapSorter(Sorter sorter)
        => sorter switch
        {
            IntegerSorter => [new SortableField(FieldNameHelper.FieldName(sorter.FieldName, Constants.FieldValues.Integers), SortType.Int)],
            DecimalSorter => [new SortableField(FieldNameHelper.FieldName(sorter.FieldName, Constants.FieldValues.Decimals), SortType.Double)],
            DateTimeOffsetSorter => [new SortableField(FieldNameHelper.FieldName(sorter.FieldName, Constants.FieldValues.DateTimeOffsets), SortType.Long)],
            KeywordSorter => [new SortableField(FieldNameHelper.QueryableKeywordFieldName(FieldNameHelper.FieldName(sorter.FieldName, Constants.FieldValues.Keywords)), SortType.String)],
            TextSorter =>
            [
                new SortableField(FieldNameHelper.FieldName(sorter.FieldName, Constants.FieldValues.TextsR1), SortType.String),
                new SortableField(FieldNameHelper.FieldName(sorter.FieldName, Constants.FieldValues.TextsR2), SortType.String),
                new SortableField(FieldNameHelper.FieldName(sorter.FieldName, Constants.FieldValues.TextsR3), SortType.String),
                new SortableField(FieldNameHelper.FieldName(sorter.FieldName, Constants.FieldValues.Texts), SortType.String),
            ],
            ScoreSorter => [],
            _ => null // Return null to indicate custom sorter handling is needed
        };

    /// <summary>
    /// Override this method to handle custom <see cref="Sorter"/> types in derived classes.
    /// This method is called for sorters that are not built-in sorter types, allowing direct
    /// manipulation of the search query.
    /// </summary>
    /// <param name="searchQuery">The search query to add sorting to.</param>
    /// <param name="sorter">The custom sorter to handle.</param>
    /// <param name="culture">The optional culture context.</param>
    /// <param name="segment">The optional segment context.</param>
    protected virtual void AddCustomSorter(IBooleanOperation searchQuery, Sorter sorter, string? culture, string? segment)
    {
        // By default, throw an exception for unknown sorter types.
        // Override in derived classes to handle custom Sorter types.
        throw new ArgumentOutOfRangeException(nameof(sorter), $"Unknown sorter type: {sorter.GetType().Name}");
    }

    private void AddFilters(IBooleanOperation searchQuery, IEnumerable<Filter>? filters, string? culture, string? segment)
    {
        if (filters is null)
        {
            return;
        }

        foreach (Filter filter in filters)
        {
            switch (filter)
            {
                case KeywordFilter keywordFilter:
                    var keywordFieldName = FieldNameHelper.FieldName(filter.FieldName, Constants.FieldValues.Keywords);

                    if (keywordFilter.Negate)
                    {
                        searchQuery.Not().GroupedOr([keywordFieldName], keywordFilter.Values);
                    }
                    else
                    {
                        searchQuery.And().GroupedOr([keywordFieldName], keywordFilter.Values);
                    }

                    break;
                case TextFilter textFilter:
                    // it would be nice to have correctly boosted wildcard search here. unfortunately, this requires the
                    // same workaround as we currently have for free text search (see the comments for free text search
                    // elsewhere in this service).
                    // for now, we will make do with wildcard text filters across all textual relevance fields, and
                    // live with not having correct relevance boost for the results.
                    List<string> textFields =
                    [
                        FieldNameHelper.FieldName(filter.FieldName, Constants.FieldValues.Texts),
                        FieldNameHelper.FieldName(filter.FieldName, Constants.FieldValues.TextsR1),
                        FieldNameHelper.FieldName(filter.FieldName, Constants.FieldValues.TextsR2),
                        FieldNameHelper.FieldName(filter.FieldName, Constants.FieldValues.TextsR3)
                    ];

                    if (segment is not null)
                    {
                        textFields.Add(FieldNameHelper.FieldName(filter.FieldName, Constants.FieldValues.Texts, segment));
                        textFields.Add(FieldNameHelper.FieldName(filter.FieldName, Constants.FieldValues.TextsR1, segment));
                        textFields.Add(FieldNameHelper.FieldName(filter.FieldName, Constants.FieldValues.TextsR2, segment));
                        textFields.Add(FieldNameHelper.FieldName(filter.FieldName, Constants.FieldValues.TextsR3, segment));
                    }

                    IExamineValue[] query = textFilter.Values
                        .SelectMany(value => new IExamineValue[]
                        {
                            new ExamineValue(Examineness.Explicit, value),
                            new ExamineValue(Examineness.ComplexWildcard, value)
                        })
                        .ToArray();
                    if (textFilter.Negate)
                    {
                        searchQuery.Not().GroupedOr(textFields, query);
                    }
                    else
                    {
                        searchQuery.And().GroupedOr(textFields, query);
                    }

                    break;
                case IntegerRangeFilter integerRangeFilter:
                    var integerRangeFieldName = FieldNameHelper.FieldName(integerRangeFilter.FieldName, Constants.FieldValues.Integers);
                    var integerRangeSegmentFieldName = segment is not null ? FieldNameHelper.FieldName(integerRangeFilter.FieldName, Constants.FieldValues.Integers, segment) : null;
                    FilterRange<int>[] integerRanges = integerRangeFilter.Ranges
                        .Select(r => new FilterRange<int>(r.MinValue ?? int.MinValue, r.MaxValue ?? int.MaxValue))
                        .ToArray();
                    searchQuery.AddRangeFilter(integerRangeFieldName, integerRangeSegmentFieldName, integerRangeFilter.Negate, integerRanges);
                    break;
                case IntegerExactFilter integerExactFilter:
                    var integerExactFieldName = FieldNameHelper.FieldName(integerExactFilter.FieldName, Constants.FieldValues.Integers);
                    var integerExactSegmentFieldName = segment is not null ? FieldNameHelper.FieldName(integerExactFilter.FieldName, Constants.FieldValues.Integers, segment) : null;
                    searchQuery.AddExactFilter(integerExactFieldName, integerExactSegmentFieldName, integerExactFilter);
                    break;
                case DecimalRangeFilter decimalRangeFilter:
                    var decimalRangeFieldName = FieldNameHelper.FieldName(decimalRangeFilter.FieldName, Constants.FieldValues.Decimals);
                    var decimalRangeSegmentFieldName = segment is not null ? FieldNameHelper.FieldName(decimalRangeFilter.FieldName, Constants.FieldValues.Decimals, segment) : null;
                    FilterRange<double>[] doubleRanges = decimalRangeFilter.Ranges
                        .Select(r => new FilterRange<double>(Convert.ToDouble(r.MinValue ?? decimal.MinValue), Convert.ToDouble(r.MaxValue ?? decimal.MaxValue)))
                        .ToArray();
                    searchQuery.AddRangeFilter(decimalRangeFieldName, decimalRangeSegmentFieldName, decimalRangeFilter.Negate, doubleRanges);
                    break;
                case DecimalExactFilter decimalExactFilter:
                    var decimalExactFieldName = FieldNameHelper.FieldName(decimalExactFilter.FieldName, Constants.FieldValues.Decimals);
                    var decimalExactSegmentFieldName = segment is not null ? FieldNameHelper.FieldName(decimalExactFilter.FieldName, Constants.FieldValues.Decimals, segment) : null;
                    var doubleExactFilter = new DoubleExactFilter(filter.FieldName, decimalExactFilter.Values.Select(xx => (double)xx).ToArray(), filter.Negate);
                    searchQuery.AddExactFilter(decimalExactFieldName, decimalExactSegmentFieldName, doubleExactFilter);
                    break;
                case DateTimeOffsetRangeFilter dateTimeOffsetRangeFilter:
                    var dateTimeOffsetRangeFieldName = FieldNameHelper.FieldName(dateTimeOffsetRangeFilter.FieldName, Constants.FieldValues.DateTimeOffsets);
                    var dateTimeOffsetRangeSegmentFieldName = segment is not null ? FieldNameHelper.FieldName(dateTimeOffsetRangeFilter.FieldName, Constants.FieldValues.DateTimeOffsets, segment) : null;
                    FilterRange<DateTime>[] dateTimeRanges = dateTimeOffsetRangeFilter.Ranges
                        .Select(r => new FilterRange<DateTime>(r.MinValue?.DateTime ?? DateTime.MinValue, r.MaxValue?.DateTime ?? DateTime.MaxValue))
                        .ToArray();
                    searchQuery.AddRangeFilter(dateTimeOffsetRangeFieldName, dateTimeOffsetRangeSegmentFieldName, dateTimeOffsetRangeFilter.Negate, dateTimeRanges);
                    break;
                case DateTimeOffsetExactFilter dateTimeOffsetExactFilter:
                    var dateTimeOffsetExactFieldName = FieldNameHelper.FieldName(dateTimeOffsetExactFilter.FieldName, Constants.FieldValues.DateTimeOffsets);
                    var dateTimeOffsetExactSegmentFieldName = segment is not null ? FieldNameHelper.FieldName(dateTimeOffsetExactFilter.FieldName, Constants.FieldValues.DateTimeOffsets, segment) : null;
                    var datetimeExactFilter = new DateTimeExactFilter(filter.FieldName, dateTimeOffsetExactFilter.Values.Select(value => value.DateTime).ToArray(), filter.Negate);
                    searchQuery.AddExactFilter(dateTimeOffsetExactFieldName, dateTimeOffsetExactSegmentFieldName, datetimeExactFilter);
                    break;
                default:
                    AddCustomFilter(searchQuery, filter, culture, segment);
                    break;
            }
        }
    }

    /// <summary>
    /// Override this method to handle custom <see cref="Filter"/> types in derived classes.
    /// This method is called for each filter that is not a built-in filter type.
    /// </summary>
    /// <param name="searchQuery">The search query to add the filter to.</param>
    /// <param name="filter">The custom filter to handle.</param>
    /// <param name="culture">The optional culture context.</param>
    /// <param name="segment">The optional segment context.</param>
    protected virtual void AddCustomFilter(IBooleanOperation searchQuery, Filter filter, string? culture, string? segment)
    {
        // No-op by default. Override in derived classes to handle custom Filter types.
    }

    private void AddFacets(IOrdering searchQuery, IEnumerable<Facet>? facets, string? culture, string? segment)
    {
        if (facets is null)
        {
            return;
        }

        searchQuery.WithFacets(facetOperations =>
        {
            foreach (Facet facet in facets)
            {
                switch (facet)
                {
                    case IntegerExactFacet integerExactFacet:
                        facetOperations.FacetString(FieldNameHelper.FieldName(integerExactFacet.FieldName, Constants.FieldValues.Integers), config => config.MaxCount(SearcherOptions.MaxFacetValues));
                        break;
                    case IntegerRangeFacet integerRangeFacet:
                        facetOperations.FacetLongRange(
                            FieldNameHelper.FieldName(integerRangeFacet.FieldName, Constants.FieldValues.Integers),
                            integerRangeFacet.Ranges
                                .Select(x =>
                                    new Int64Range(x.Key, x.MinValue ?? 0, true, x.MaxValue ?? int.MaxValue, false))
                                .ToArray());
                        break;
                    case DecimalExactFacet decimalExactFacet:
                        facetOperations.FacetString(FieldNameHelper.FieldName(decimalExactFacet.FieldName, Constants.FieldValues.Decimals), config => config.MaxCount(SearcherOptions.MaxFacetValues));
                        break;
                    case DecimalRangeFacet decimalRangeFacet:
                    {
                        DoubleRange[] doubleRanges = decimalRangeFacet.Ranges.Select(x =>
                                new DoubleRange(
                                    x.Key,
                                    decimal.ToDouble(x.MinValue ?? 0),
                                    true,
                                    decimal.ToDouble(x.MaxValue ?? 0),
                                    false))
                            .ToArray();
                        facetOperations.FacetDoubleRange(
                            FieldNameHelper.FieldName(decimalRangeFacet.FieldName, Constants.FieldValues.Decimals),
                            doubleRanges);
                        break;
                    }
                    case DateTimeOffsetExactFacet dateTimeOffsetExactFacet:
                        facetOperations.FacetString(FieldNameHelper.FieldName(dateTimeOffsetExactFacet.FieldName, Constants.FieldValues.DateTimeOffsets), config => config.MaxCount(SearcherOptions.MaxFacetValues));
                        break;
                    case DateTimeOffsetRangeFacet dateTimeOffsetRangeFacet:
                        facetOperations.FacetLongRange(
                            FieldNameHelper.FieldName(dateTimeOffsetRangeFacet.FieldName, Constants.FieldValues.DateTimeOffsets),
                            dateTimeOffsetRangeFacet.Ranges.Select(x => new Int64Range(
                                x.Key,
                                x.MinValue?.Ticks ?? DateTime.MinValue.Ticks,
                                true,
                                x.MaxValue?.Ticks ?? DateTime.MaxValue.Ticks,
                                false))
                                .ToArray());
                        break;
                    case KeywordFacet keywordFacet:
                        var keywordFieldName = FieldNameHelper.QueryableKeywordFieldName(FieldNameHelper.FieldName(keywordFacet.FieldName, Constants.FieldValues.Keywords));
                        facetOperations.FacetString(keywordFieldName, config => config.MaxCount(SearcherOptions.MaxFacetValues));
                        break;
                    default:
                        AddCustomFacet(facetOperations, facet, culture, segment);
                        break;
                }
            }
        });
    }

    /// <summary>
    /// Override this method to handle custom <see cref="Facet"/> types in derived classes.
    /// This method is called for each facet that is not a built-in facet type.
    /// </summary>
    /// <param name="facetOperations">The facet operations to add the facet to.</param>
    /// <param name="facet">The custom facet to handle.</param>
    /// <param name="culture">The optional culture context.</param>
    /// <param name="segment">The optional segment context.</param>
    protected virtual void AddCustomFacet(IFacetOperations facetOperations, Facet facet, string? culture, string? segment)
    {
        // No-op by default. Override in derived classes to handle custom Facet types.
    }

    private Facet[] DeduplicateFacets(IEnumerable<Facet>? facets)
    {
        if (facets is null)
        {
            return [];
        }

        return facets
            .GroupBy(f => (f.FieldName, f.GetType())) // group by field + facet type
            .Select(group =>
            {
                Facet first = group.First();

                return first.GetType().IsSubclassOf(typeof(RangeFacet<>)) ? MergeRangeFacets(group) : first;
            })
            .ToArray();
    }

    private Facet MergeRangeFacets(IEnumerable<Facet> facets)
    {
        Facet first = facets.First();
        Type type = first.GetType();

        PropertyInfo rangesProperty = type.GetProperty("Ranges")!;
        var allRanges = facets
            .SelectMany(f => (IEnumerable<object>)rangesProperty.GetValue(f)!)
            .Distinct()
            .ToArray();

        // Construct new facet with FieldName + merged ranges
        return (Facet)Activator.CreateInstance(type, first.FieldName, allRanges)!;
    }


    private static Document? MapToDocument(ISearchResult item)
    {
        var objectTypeString = item.Values.GetValueOrDefault(ExamineFieldNames.CategoryFieldName);

        Enum.TryParse(objectTypeString, out UmbracoObjectTypes umbracoObjectType);

        if (Guid.TryParse(item.Id, out Guid guidId))
        {
            return new Document(guidId, umbracoObjectType);
        }

        // The id of an item may be appended with _{culture_{segment}, so strip those and map to guid.
        var indexofUnderscore = item.Id.IndexOf('_');
        var idWithOutCulture = item.Id.Remove(indexofUnderscore);
        return Guid.TryParse(idWithOutCulture, out Guid idWithoutCultureGuid)
            ? new Document(idWithoutCultureGuid, umbracoObjectType)
            : null;
    }

    private IEnumerable<FacetResult> MapFacets(ISearchResults searchResults, IEnumerable<Facet> queryFacets, string? culture, string? segment)
    {
        foreach (Facet facet in queryFacets)
        {
            switch (facet)
            {
                case IntegerRangeFacet integerRangeFacet:
                {
                    IEnumerable<IntegerRangeFacetValue> integerRangeFacetResult = integerRangeFacet.Ranges.Select(x =>
                    {
                        int value = GetFacetCount(FieldNameHelper.FieldName(integerRangeFacet.FieldName, Constants.FieldValues.Integers), x.Key, searchResults);
                        return new IntegerRangeFacetValue(x.Key, x.MinValue, x.MaxValue, value);
                    });
                    yield return new FacetResult(facet.FieldName, integerRangeFacetResult);
                    break;
                }
                case IntegerExactFacet integerExactFacet:
                    IFacetResult? examineIntegerFacets = searchResults.GetFacet(FieldNameHelper.FieldName(integerExactFacet.FieldName, Constants.FieldValues.Integers));
                    if (examineIntegerFacets is null)
                    {
                        continue;
                    }

                    var integerExactFacetValues = new List<IntegerExactFacetValue>();
                    foreach (IFacetValue integerExactFacetValue in examineIntegerFacets)
                    {
                        if (int.TryParse(integerExactFacetValue.Label, out var labelValue) is false)
                        {
                            // Cannot convert the label to int, skipping.
                            continue;
                        }
                        integerExactFacetValues.Add(new IntegerExactFacetValue(labelValue, (int)integerExactFacetValue.Value));
                    }

                    yield return new FacetResult(facet.FieldName, integerExactFacetValues.OrderBy(x => x.Key));
                    break;
                case DecimalRangeFacet decimalRangeFacet:
                    IEnumerable<DecimalRangeFacetValue> decimalRangeFacetResult = decimalRangeFacet.Ranges.Select(x =>
                    {
                        int value = GetFacetCount(FieldNameHelper.FieldName(decimalRangeFacet.FieldName, Constants.FieldValues.Decimals), x.Key, searchResults);
                        return new DecimalRangeFacetValue(x.Key, x.MinValue, x.MaxValue, value);
                    });
                    yield return new FacetResult(facet.FieldName, decimalRangeFacetResult);
                    break;
                case DecimalExactFacet decimalExactFacet:
                    IFacetResult? examineDecimalFacets = searchResults.GetFacet(FieldNameHelper.FieldName(decimalExactFacet.FieldName, Constants.FieldValues.Decimals));
                    if (examineDecimalFacets is null)
                    {
                        continue;
                    }

                    var decimalExactFacetValues = new List<DecimalExactFacetValue>();

                    foreach (IFacetValue decimalExactFacetValue in examineDecimalFacets)
                    {
                        if (decimal.TryParse(decimalExactFacetValue.Label, out var labelValue) is false)
                        {
                            // Cannot convert the label to decimal, skipping.
                            continue;
                        }
                        decimalExactFacetValues.Add(new DecimalExactFacetValue(labelValue, (int)decimalExactFacetValue.Value));
                    }

                    yield return new FacetResult(facet.FieldName, decimalExactFacetValues.OrderBy(x => x.Key));
                    break;
                case DateTimeOffsetRangeFacet dateTimeOffsetRangeFacet:
                    IEnumerable<DateTimeOffsetRangeFacetValue> dateTimeOffsetRangeFacetResult = dateTimeOffsetRangeFacet.Ranges.Select(x =>
                    {
                        int value = GetFacetCount(FieldNameHelper.FieldName(dateTimeOffsetRangeFacet.FieldName, Constants.FieldValues.DateTimeOffsets), x.Key, searchResults);
                        return new DateTimeOffsetRangeFacetValue(x.Key, x.MinValue, x.MaxValue, value);
                    });
                    yield return new FacetResult(facet.FieldName, dateTimeOffsetRangeFacetResult);
                    break;
                case DateTimeOffsetExactFacet dateTimeOffsetExactFacet:
                    IFacetResult? examineDatetimeFacets = searchResults.GetFacet(FieldNameHelper.FieldName(dateTimeOffsetExactFacet.FieldName, Constants.FieldValues.DateTimeOffsets));
                    if (examineDatetimeFacets is null)
                    {
                        continue;
                    }

                    var datetimeOffsetExactFacetValues = new List<DateTimeOffsetExactFacetValue>();

                    foreach (IFacetValue datetimeExactFacetValue in examineDatetimeFacets)
                    {
                        if (long.TryParse(datetimeExactFacetValue.Label, out var ticks) is false)
                        {
                            // Cannot convert the label to ticks (long), skipping.
                            continue;
                        }

                        DateTimeOffset offSet = new DateTimeOffset().AddTicks(ticks);
                        datetimeOffsetExactFacetValues.Add(new DateTimeOffsetExactFacetValue(offSet, (int)datetimeExactFacetValue.Value));
                    }

                    yield return new FacetResult(facet.FieldName, datetimeOffsetExactFacetValues.OrderBy(x => x.Key));
                    break;
                case KeywordFacet keywordFacet:
                    IFacetResult? examineKeywordFacets = searchResults.GetFacet(FieldNameHelper.QueryableKeywordFieldName(FieldNameHelper.FieldName(keywordFacet.FieldName, Constants.FieldValues.Keywords)));
                    if (examineKeywordFacets is null)
                    {
                        continue;
                    }

                    var keywordFacetValues = examineKeywordFacets.Select(examineKeywordFacet => new KeywordFacetValue(examineKeywordFacet.Label, (int)examineKeywordFacet.Value)).ToList();
                    yield return new FacetResult(facet.FieldName, keywordFacetValues);
                    break;
                default:
                    FacetResult? customFacetResult = ExtractCustomFacetResult(facet, searchResults, culture, segment);
                    if (customFacetResult is not null)
                    {
                        yield return customFacetResult;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Override this method to extract custom <see cref="Facet"/> types to <see cref="FacetResult"/> in derived classes.
    /// This method is called for each facet result that is not a built-in facet type.
    /// </summary>
    /// <param name="facet">The custom facet to extract results for.</param>
    /// <param name="searchResults">The search results containing the facet data.</param>
    /// <param name="culture">The optional culture context.</param>
    /// <param name="segment">The optional segment context.</param>
    /// <returns>A <see cref="FacetResult"/> for the custom facet, or null to skip.</returns>
    protected virtual FacetResult? ExtractCustomFacetResult(Facet facet, ISearchResults searchResults, string? culture, string? segment)
    {
        // No-op by default. Override in derived classes to handle custom Facet types.
        return null;
    }

    private static int GetFacetCount(string fieldName, string key, ISearchResults results)
        => (int?)results.GetFacet(fieldName)?.Facet(key)?.Value ?? 0;

    private INestedBooleanOperation CreateAggregatedTextQuery(INestedQuery nestedQuery, string term, string? searchSegment)
    {
        // Both an analyzed exact-match clause and a wildcard clause are needed per relevance tier:
        // wildcard queries bypass the analyzer, so they cannot match indexed terms that were
        // tokenized at script boundaries (e.g. Japanese katakana + Latin in "ボディSegment1").
        // The analyzed Boost() clause provides token-aware matching; the wildcard clause provides
        // prefix matching. Both carry the tier boost.
        INestedBooleanOperation result = nestedQuery
            .Field(
                Constants.SystemFields.AggregatedTextsR1,
                term.Boost(SearcherOptions.BoostFactorTextR1))
            .Or()
            .Field(
                Constants.SystemFields.AggregatedTextsR1,
                term.MultipleCharacterWildcard().WithBoost(SearcherOptions.BoostFactorTextR1))
            .Or()
            .Field(
                Constants.SystemFields.AggregatedTextsR2,
                term.Boost(SearcherOptions.BoostFactorTextR2))
            .Or()
            .Field(
                Constants.SystemFields.AggregatedTextsR2,
                term.MultipleCharacterWildcard().WithBoost(SearcherOptions.BoostFactorTextR2))
            .Or()
            .Field(
                Constants.SystemFields.AggregatedTextsR3,
                term.Boost(SearcherOptions.BoostFactorTextR3))
            .Or()
            .Field(
                Constants.SystemFields.AggregatedTextsR3,
                term.MultipleCharacterWildcard().WithBoost(SearcherOptions.BoostFactorTextR3))
            .Or()
            .Field(
                Constants.SystemFields.AggregatedTexts,
                term.Boost(1.0f))
            .Or()
            .Field(
                Constants.SystemFields.AggregatedTexts,
                term.MultipleCharacterWildcard().WithBoost(1.0f));

        if (searchSegment is not null)
        {
            result
                .Or()
                .Field(
                    FieldNameHelper.SegmentedSystemFieldName(Constants.SystemFields.AggregatedTextsR1, searchSegment),
                    term.Boost(SearcherOptions.BoostFactorTextR1))
                .Or()
                .Field(
                    FieldNameHelper.SegmentedSystemFieldName(Constants.SystemFields.AggregatedTextsR1, searchSegment),
                    term.MultipleCharacterWildcard().WithBoost(SearcherOptions.BoostFactorTextR1))
                .Or()
                .Field(
                    FieldNameHelper.SegmentedSystemFieldName(Constants.SystemFields.AggregatedTextsR2, searchSegment),
                    term.Boost(SearcherOptions.BoostFactorTextR2))
                .Or()
                .Field(
                    FieldNameHelper.SegmentedSystemFieldName(Constants.SystemFields.AggregatedTextsR2, searchSegment),
                    term.MultipleCharacterWildcard().WithBoost(SearcherOptions.BoostFactorTextR2))
                .Or()
                .Field(
                    FieldNameHelper.SegmentedSystemFieldName(Constants.SystemFields.AggregatedTextsR3, searchSegment),
                    term.Boost(SearcherOptions.BoostFactorTextR3))
                .Or()
                .Field(
                    FieldNameHelper.SegmentedSystemFieldName(Constants.SystemFields.AggregatedTextsR3, searchSegment),
                    term.MultipleCharacterWildcard().WithBoost(SearcherOptions.BoostFactorTextR3))
                .Or()
                .Field(
                    FieldNameHelper.SegmentedSystemFieldName(Constants.SystemFields.AggregatedTexts, searchSegment),
                    term.Boost(1.0f))
                .Or()
                .Field(
                    FieldNameHelper.SegmentedSystemFieldName(Constants.SystemFields.AggregatedTexts, searchSegment),
                    term.MultipleCharacterWildcard().WithBoost(1.0f));
        }

        return result;
    }

    private static void EnsureFieldAnalyzersLoaded(IIndex index)
    {
        if (InitializedIndexes.TryGetValue(index.Name, out _))
        {
            return;
        }

        // Doing a search forces Examine to load per-field analyzers (e.g. KeywordAnalyzer for
        // RawStringType fields). Without this, the first GroupedOr/Field calls after startup
        // use the default StandardAnalyzer, which tokenizes values on hyphens and other
        // punctuation — causing 0 results on keyword fields that store raw values.
        index.Searcher.Search(string.Empty, QueryOptions.SkipTake(0, 0));
        InitializedIndexes.TryAdd(index.Name, true);
    }

    /// <summary>
    /// Gets suggestions for autocomplete based on the query.
    /// Override this method to provide custom suggestion logic.
    /// </summary>
    /// <param name="indexAlias">The index alias to search in.</param>
    /// <param name="query">The search query.</param>
    /// <param name="culture">The culture to search in.</param>
    /// <param name="segment">The segment to search in.</param>
    /// <param name="maxSuggestions">The maximum number of suggestions to return.</param>
    /// <returns>A list of suggestions, empty by default.</returns>
    protected virtual Task<IEnumerable<string>> GetSuggestionsAsync(
        string indexAlias,
        string? query,
        string? culture,
        string? segment,
        int maxSuggestions)
    {
        return Task.FromResult<IEnumerable<string>>([]);
    }
}
