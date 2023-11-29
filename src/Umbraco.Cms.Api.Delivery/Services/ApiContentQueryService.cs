using Umbraco.Cms.Api.Delivery.Indexing.Selectors;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Delivery.Services;

internal sealed class ApiContentQueryService : IApiContentQueryService
{
    private readonly IRequestStartItemProviderAccessor _requestStartItemProviderAccessor;
    private readonly SelectorHandlerCollection _selectorHandlers;
    private readonly FilterHandlerCollection _filterHandlers;
    private readonly SortHandlerCollection _sortHandlers;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IApiContentQueryProvider _apiContentQueryProvider;
    private readonly IRequestPreviewService _requestPreviewService;

    public ApiContentQueryService(
        IRequestStartItemProviderAccessor requestStartItemProviderAccessor,
        SelectorHandlerCollection selectorHandlers,
        FilterHandlerCollection filterHandlers,
        SortHandlerCollection sortHandlers,
        IVariationContextAccessor variationContextAccessor,
        IApiContentQueryProvider apiContentQueryProvider,
        IRequestPreviewService requestPreviewService)
    {
        _requestStartItemProviderAccessor = requestStartItemProviderAccessor;
        _selectorHandlers = selectorHandlers;
        _filterHandlers = filterHandlers;
        _sortHandlers = sortHandlers;
        _variationContextAccessor = variationContextAccessor;
        _apiContentQueryProvider = apiContentQueryProvider;
        _requestPreviewService = requestPreviewService;
    }

    [Obsolete($"Use the {nameof(ExecuteQuery)} method that accepts {nameof(ProtectedAccess)}. Will be removed in V14.")]
    public Attempt<PagedModel<Guid>, ApiContentQueryOperationStatus> ExecuteQuery(
        string? fetch,
        IEnumerable<string> filters,
        IEnumerable<string> sorts,
        int skip,
        int take)
        => ExecuteQuery(fetch, filters, sorts, ProtectedAccess.None, skip, take);

    /// <inheritdoc/>
    public Attempt<PagedModel<Guid>, ApiContentQueryOperationStatus> ExecuteQuery(
        string? fetch,
        IEnumerable<string> filters,
        IEnumerable<string> sorts,
        ProtectedAccess protectedAccess,
        int skip,
        int take)
    {
        var emptyResult = new PagedModel<Guid>();

        SelectorOption? selectorOption = GetSelectorOption(fetch);
        if (selectorOption is null)
        {
            // If no Selector could be found, we return no results
            return Attempt.FailWithStatus(ApiContentQueryOperationStatus.SelectorOptionNotFound, emptyResult);
        }

        var filterOptions = new List<FilterOption>();
        foreach (var filter in filters.Where(filter => filter.IsNullOrWhiteSpace() is false))
        {
            FilterOption? filterOption = GetFilterOption(filter);
            if (filterOption is null)
            {
                // If there is an invalid Filter option, we return no results
                return Attempt.FailWithStatus(ApiContentQueryOperationStatus.FilterOptionNotFound, emptyResult);
            }

            filterOptions.Add(filterOption);
        }

        var sortOptions = new List<SortOption>();
        foreach (var sort in sorts.Where(sort => sort.IsNullOrWhiteSpace() is false))
        {
            SortOption? sortOption = GetSortOption(sort);
            if (sortOption is null)
            {
                // If there is an invalid Sort option, we return no results
                return Attempt.FailWithStatus(ApiContentQueryOperationStatus.SortOptionNotFound, emptyResult);
            }

            sortOptions.Add(sortOption);
        }

        var culture = _variationContextAccessor.VariationContext?.Culture ?? string.Empty;
        var isPreview = _requestPreviewService.IsPreview();

        PagedModel<Guid> result = _apiContentQueryProvider.ExecuteQuery(selectorOption, filterOptions, sortOptions, culture, protectedAccess, isPreview, skip, take);
        return Attempt.SucceedWithStatus(ApiContentQueryOperationStatus.Success, result);
    }

    private SelectorOption? GetSelectorOption(string? fetch)
    {
        if (fetch is not null)
        {
            ISelectorHandler? selectorHandler = _selectorHandlers.FirstOrDefault(h => h.CanHandle(fetch));
            return selectorHandler?.BuildSelectorOption(fetch);
        }

        if (_requestStartItemProviderAccessor.TryGetValue(out IRequestStartItemProvider? requestStartItemProvider))
        {
            IPublishedContent? startItem = requestStartItemProvider.GetStartItem();
            if (startItem is not null)
            {
                // Reusing the boolean operation of the "Descendants" selector, as we want to get all the nodes from the given starting point
                return new SelectorOption
                {
                    FieldName = DescendantsSelectorIndexer.FieldName, Values = new[] { startItem.Key.ToString() }
                };
            }
        }

        return _apiContentQueryProvider.AllContentSelectorOption();
    }

    private FilterOption? GetFilterOption(string filter)
    {
        IFilterHandler? filterHandler = _filterHandlers.FirstOrDefault(h => h.CanHandle(filter));
        return filterHandler?.BuildFilterOption(filter);
    }

    private SortOption? GetSortOption(string sort)
    {
        ISortHandler? sortHandler = _sortHandlers.FirstOrDefault(h => h.CanHandle(sort));
        return sortHandler?.BuildSortOption(sort);
    }
}
