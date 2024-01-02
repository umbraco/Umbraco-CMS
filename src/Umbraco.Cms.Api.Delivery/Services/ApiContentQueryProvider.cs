using Examine;
using Examine.Search;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Delivery.Services.QueryBuilders;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.Api.Delivery.Services;

/// <summary>
/// This is the Examine implementation of content querying for the Delivery API.
/// </summary>
internal sealed class ApiContentQueryProvider : IApiContentQueryProvider
{
    private const string ItemIdFieldName = "itemId";
    private readonly IExamineManager _examineManager;
    private readonly ILogger<ApiContentQueryProvider> _logger;
    private readonly ApiContentQuerySelectorBuilder _selectorBuilder;
    private readonly ApiContentQueryFilterBuilder _filterBuilder;
    private readonly ApiContentQuerySortBuilder _sortBuilder;

    public ApiContentQueryProvider(
        IExamineManager examineManager,
        ContentIndexHandlerCollection indexHandlers,
        IOptions<DeliveryApiSettings> deliveryApiSettings,
        ILogger<ApiContentQueryProvider> logger)
    {
        _examineManager = examineManager;
        _logger = logger;

        // build a look-up dictionary of field types by field name
        var fieldTypes = indexHandlers
            .SelectMany(handler => handler.GetFields())
            .DistinctBy(field => field.FieldName)
            .ToDictionary(field => field.FieldName, field => field.FieldType, StringComparer.InvariantCultureIgnoreCase);

        // for the time being we're going to keep these as internal implementation details.
        // perhaps later on it will make sense to expose them through the DI.
        _selectorBuilder = new ApiContentQuerySelectorBuilder(deliveryApiSettings.Value);
        _filterBuilder = new ApiContentQueryFilterBuilder(fieldTypes, _logger);
        _sortBuilder = new ApiContentQuerySortBuilder(fieldTypes, _logger);

    }

    [Obsolete($"Use the {nameof(ExecuteQuery)} method that accepts {nameof(ProtectedAccess)}. Will be removed in V14.")]
    public PagedModel<Guid> ExecuteQuery(
        SelectorOption selectorOption,
        IList<FilterOption> filterOptions,
        IList<SortOption> sortOptions,
        string culture,
        bool preview,
        int skip,
        int take)
        => ExecuteQuery(selectorOption, filterOptions, sortOptions, culture, ProtectedAccess.None, preview, skip, take);

    /// <inheritdoc/>
    public PagedModel<Guid> ExecuteQuery(
        SelectorOption selectorOption,
        IList<FilterOption> filterOptions,
        IList<SortOption> sortOptions,
        string culture,
        ProtectedAccess protectedAccess,
        bool preview,
        int skip,
        int take)
    {
        if (!_examineManager.TryGetIndex(Constants.UmbracoIndexes.DeliveryApiContentIndexName, out IIndex? index))
        {
            _logger.LogError("Could not find the index {IndexName} when attempting to execute a query.", Constants.UmbracoIndexes.DeliveryApiContentIndexName);
            return new PagedModel<Guid>();
        }

        IBooleanOperation queryOperation = _selectorBuilder.Build(selectorOption, index, culture, protectedAccess, preview);
        _filterBuilder.Append(filterOptions, queryOperation);
        _sortBuilder.Append(sortOptions, queryOperation);

        ISearchResults? results = queryOperation
            .SelectField(ItemIdFieldName)
            .Execute(QueryOptions.SkipTake(skip, take));

        if (results is null)
        {
            // The query yield no results
            return new PagedModel<Guid>();
        }

        Guid[] items = results
            .Where(r => r.Values.ContainsKey(ItemIdFieldName))
            .Select(r => Guid.Parse(r.Values[ItemIdFieldName]))
            .ToArray();

        return new PagedModel<Guid>(results.TotalItemCount, items);
    }

    public SelectorOption AllContentSelectorOption() => new()
    {
        FieldName = UmbracoExamineFieldNames.CategoryFieldName, Values = new[] { "content" }
    };
}
