using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Services;

internal abstract class ContentListViewServiceBase<TContent, TContentType, TContentTypeService>
    where TContent : class, IContentBase
    where TContentType : class, IContentTypeComposition
    where TContentTypeService : IContentTypeBaseService<TContentType>
{
    private readonly TContentTypeService _contentTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IContentSearchService<TContent> _contentSearchService;

    protected ContentListViewServiceBase(TContentTypeService contentTypeService, IDataTypeService dataTypeService, IContentSearchService<TContent> contentSearchService)
    {
        _contentTypeService = contentTypeService;
        _dataTypeService = dataTypeService;
        _contentSearchService = contentSearchService;
    }

    protected abstract Guid DefaultListViewKey { get; }

    protected abstract Task<bool> HasAccessToListViewItemAsync(IUser user, Guid key);

    protected async Task<Attempt<ListViewPagedModel<TContent>?, ContentCollectionOperationStatus>> GetListViewResultAsync(
        IUser user,
        TContent? content,
        Guid? dataTypeKey,
        string orderBy,
        string? orderCulture,
        Direction orderDirection,
        string? filter,
        int skip,
        int take)
    {
        Attempt<ListViewConfiguration?, ContentCollectionOperationStatus> configurationAttempt = await GetListViewConfigurationAsync(content?.ContentType.Key, dataTypeKey);

        if (configurationAttempt.Success == false)
        {
            return Attempt.FailWithStatus<ListViewPagedModel<TContent>?, ContentCollectionOperationStatus>(configurationAttempt.Status, null);
        }

        Attempt<Ordering?, ContentCollectionOperationStatus> orderingAttempt = HandleListViewOrdering(configurationAttempt.Result, orderBy, orderCulture, orderDirection);

        if (orderingAttempt.Success == false)
        {
            return Attempt.FailWithStatus<ListViewPagedModel<TContent>?, ContentCollectionOperationStatus>(orderingAttempt.Status, null);
        }

        // Extract non-system property aliases from configuration to optimize property loading (we'll optimize and only
        // load the properties we need to populate the collection view).
        string[]? customPropertyAliases = ExtractCustomPropertyAliases(configurationAttempt.Result, orderingAttempt.Result);

        PagedModel<TContent> items = await GetAllowedListViewItemsAsync(user, content?.Key, filter, orderingAttempt.Result, customPropertyAliases, skip, take);

        var result = new ListViewPagedModel<TContent>
        {
            Items = items,
            ListViewConfiguration = configurationAttempt.Result!,
        };

        return Attempt.SucceedWithStatus<ListViewPagedModel<TContent>?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.Success, result);
    }

    private Attempt<Ordering?, ContentCollectionOperationStatus> HandleListViewOrdering(
        ListViewConfiguration? listViewConfiguration,
        string orderBy,
        string? orderCulture,
        Direction orderDirection)
    {
        ListViewConfiguration.Property[]? listViewProperties = listViewConfiguration?.IncludeProperties;

        if (listViewProperties == null || listViewProperties.Length == 0)
        {
            return Attempt.FailWithStatus<Ordering?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.MissingPropertiesInCollectionConfiguration, null);
        }

        IEnumerable<string> listViewPropertyAliases = listViewProperties
            .Select(p => p.Alias)
            .WhereNotNull();

        if (listViewPropertyAliases.Contains(orderBy) == false && orderBy.InvariantEquals("name") == false)
        {
            return Attempt.FailWithStatus<Ordering?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.OrderByNotPartOfCollectionConfiguration, null);
        }

        // Service layer expects "owner" instead of "creator", so make sure to pass in the correct field
        if (orderBy.InvariantEquals("creator"))
        {
            orderBy = "owner";
        }

        var orderByCustomField = listViewProperties
            .Any(p => p.Alias == orderBy && p.IsSystem is false);

        var ordering = Ordering.By(
            orderBy,
            orderDirection,
            orderCulture,
            orderByCustomField);

        return Attempt.SucceedWithStatus<Ordering?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.Success, ordering);
    }

    /// <summary>
    ///     Gets the list view configuration from a content type or from a specific list view data type used as a content type property.
    /// </summary>
    /// <param name="contentTypeKey">The key of the content type to check for the configured list view or for its list view properties.</param>
    /// <param name="dataTypeKey">The key of the data type used as a list view property on a content type.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="ContentCollectionOperationStatus"/>.</returns>
    /// <remarks>
    ///     dataTypeKey is ONLY used to check against list views used as content type properties. It is NOT the key for the configured list view on the content type.
    ///     To get the configured list view for a content type, you shouldn't specify dataTypeKey.
    /// </remarks>
    private async Task<Attempt<ListViewConfiguration?, ContentCollectionOperationStatus>> GetListViewConfigurationAsync(Guid? contentTypeKey, Guid? dataTypeKey)
    {
        Attempt<TContentType?, ContentCollectionOperationStatus> contentTypeAttempt = await GetContentTypeForListViewConfigurationAsync(contentTypeKey, dataTypeKey);

        if (contentTypeAttempt.Success == false)
        {
            return Attempt.FailWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(contentTypeAttempt.Status, null);
        }

        // Can be null if we are looking for items at root
        TContentType? contentType = contentTypeAttempt.Result;
        Attempt<ListViewConfiguration?, ContentCollectionOperationStatus> listViewConfigurationAttempt;

        if (dataTypeKey.HasValue && contentType != null)
        {
            listViewConfigurationAttempt = await GetListViewConfigurationFromDataTypeAsync(dataTypeKey.Value, contentType);
        }
        else
        {
            listViewConfigurationAttempt = await GetListViewConfigurationFromContentTypeAsync(contentType);
        }

        return listViewConfigurationAttempt.Success
            ? Attempt.SucceedWithStatus(ContentCollectionOperationStatus.Success, listViewConfigurationAttempt.Result)
            : listViewConfigurationAttempt;
    }

    private async Task<Attempt<TContentType?, ContentCollectionOperationStatus>> GetContentTypeForListViewConfigurationAsync(Guid? contentTypeKey, Guid? dataTypeKey)
    {
        TContentType? contentType = null;

        if (contentTypeKey.HasValue == false)
        {
            return dataTypeKey.HasValue
                ? Attempt.FailWithStatus<TContentType?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.DataTypeWithoutContentType, null)
                : Attempt.SucceedWithStatus(ContentCollectionOperationStatus.Success, contentType); // Even though we return null here, this is still valid for the case of querying for items at root.
        }

        contentType = await _contentTypeService.GetAsync(contentTypeKey.Value);

        return contentType == null
            ? Attempt.FailWithStatus<TContentType?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.ContentTypeNotFound, null)
            : Attempt.SucceedWithStatus<TContentType?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.Success, contentType);
    }

    private async Task<Attempt<ListViewConfiguration?, ContentCollectionOperationStatus>> GetListViewConfigurationFromDataTypeAsync(Guid dataTypeKey, TContentType contentType)
    {
        IDataType? dataType = await _dataTypeService.GetAsync(dataTypeKey);
        if (dataType == null)
        {
            return Attempt.FailWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.DataTypeNotFound, null);
        }

        if (dataType.ConfigurationObject is not ListViewConfiguration listViewConfiguration)
        {
            return Attempt.FailWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.DataTypeNotCollection, null);
        }

        // Check if the list view data type is a content type property
        return contentType.CompositionPropertyTypes.Any(pt => pt.DataTypeKey == dataTypeKey)
            ? Attempt.SucceedWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.Success, listViewConfiguration)
            : Attempt.FailWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.DataTypeNotContentProperty, null);
    }

    private async Task<Attempt<ListViewConfiguration?, ContentCollectionOperationStatus>> GetListViewConfigurationFromContentTypeAsync(TContentType? contentType)
    {
        if (contentType is not null && contentType.ListView is null)
        {
            return Attempt.FailWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.ContentNotCollection, null);
        }

        IDataType? currentListViewDataType = await GetConfiguredListViewDataTypeAsync(contentType);

        return currentListViewDataType?.ConfigurationObject is ListViewConfiguration listViewConfiguration
            ? Attempt.SucceedWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.Success, listViewConfiguration)
            : Attempt.FailWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.CollectionNotFound, null);
    }

    private async Task<IDataType?> GetConfiguredListViewDataTypeAsync(TContentType? contentType)
    {
        // When contentType is not configured as a list view
        if (contentType is not null && contentType.ListView is null)
        {
            return null;
        }

        // When we don't have a contentType (i.e. when root), we will get the default list view
        Guid configuredListViewKey = contentType?.ListView ?? DefaultListViewKey;

        return await _dataTypeService.GetAsync(configuredListViewKey);
    }

    /// <summary>
    ///     Extracts non-system property aliases from the list view configuration.
    /// </summary>
    /// <param name="configuration">The list view configuration.</param>
    /// <param name="ordering">The ordering information (to ensure the order-by field is included if it's a custom property).</param>
    /// <returns>
    ///     An array of custom property aliases to load. Returns empty array if only system properties are configured.
    /// </returns>
    private static string[]? ExtractCustomPropertyAliases(ListViewConfiguration? configuration, Ordering? ordering)
    {
        if (configuration?.IncludeProperties is null)
        {
            return null;
        }

        // Extract non-system property aliases.
        var customAliases = configuration.IncludeProperties
            .Where(p => p.IsSystem is false && p.Alias is not null)
            .Select(p => p.Alias!)
            .ToList();

        // If ordering by a custom field, ensure it's included in the aliases
        // (in case it's not in the configured display columns but is used for sorting).
        if (ordering?.IsCustomField is true &&
            string.IsNullOrEmpty(ordering.OrderBy) is false &&
            customAliases.Contains(ordering.OrderBy, StringComparer.OrdinalIgnoreCase) is false)
        {
            customAliases.Add(ordering.OrderBy);
        }

        return [.. customAliases];
    }

    private async Task<PagedModel<TContent>> GetAllowedListViewItemsAsync(IUser user, Guid? contentId, string? filter, Ordering? ordering, string[]? propertyAliases, int skip, int take)
    {
        // Collection views don't need templates loaded, so we pass loadTemplates: false for performance
        PagedModel<TContent> pagedChildren = await _contentSearchService.SearchChildrenAsync(filter, contentId, propertyAliases, ordering, loadTemplates: false, skip, take);

        // Filtering out child nodes after getting a paged result is an active choice here, even though the pagination might get off.
        // This has been the case with this functionality in Umbraco for a long time.
        IEnumerable<TContent> items = await FilterItemsBasedOnAccessAsync(user, pagedChildren.Items);

        var pagedResult = new PagedModel<TContent>
        {
            Items = items,
            Total = pagedChildren.Total,
        };

        return pagedResult;
    }

    // TODO: Optimize the way we filter out only the nodes the user is allowed to see - instead of checking one by one
    private async Task<IEnumerable<TContent>> FilterItemsBasedOnAccessAsync(IUser user, IEnumerable<TContent> items)
    {
        var filteredItems = new List<TContent>();

        foreach (TContent item in items)
        {
            var hasAccess = await HasAccessToListViewItemAsync(user, item.Key);

            if (hasAccess)
            {
                filteredItems.Add(item);
            }
        }

        return filteredItems;
    }
}
