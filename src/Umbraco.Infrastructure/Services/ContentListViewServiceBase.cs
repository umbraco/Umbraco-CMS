using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Services;

internal abstract class ContentListViewServiceBase<TContent, TContentType, TContentTypeService>
    where TContent : class, IContentBase
    where TContentType : class, IContentTypeComposition
    where TContentTypeService : IContentTypeBaseService<TContentType>
{
    private readonly TContentTypeService _contentTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly ISqlContext _sqlContext;

    protected ContentListViewServiceBase(TContentTypeService contentTypeService, IDataTypeService dataTypeService, ISqlContext sqlContext)
    {
        _contentTypeService = contentTypeService;
        _dataTypeService = dataTypeService;
        _sqlContext = sqlContext;
    }

    protected abstract Guid DefaultListViewKey { get; }

    protected abstract Task<PagedModel<TContent>> GetPagedChildrenAsync(
        int id,
        IQuery<TContent>? filter,
        Ordering? ordering,
        int skip,
        int take);

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

        PagedModel<TContent> items = await GetAllowedListViewItemsAsync(user, content?.Id ?? Constants.System.Root, filter, orderingAttempt.Result, skip, take);

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
        var listViewProperties = listViewConfiguration?.IncludeProperties;

        if (listViewProperties == null || listViewProperties.Length == 0)
        {
            return Attempt.FailWithStatus<Ordering?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.MissingPropertiesInCollectionConfiguration, null);
        }

        var listViewPropertyAliases = listViewProperties
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

    private async Task<PagedModel<TContent>> GetAllowedListViewItemsAsync(IUser user, int contentId, string? filter, Ordering? ordering, int skip, int take)
    {
        var queryFilter = ParseQueryFilter(filter);

        var pagedChildren = await GetPagedChildrenAsync(
            contentId,
            queryFilter,
            ordering,
            skip,
            take);

        // Filtering out child nodes after getting a paged result is an active choice here, even though the pagination might get off.
        // This has been the case with this functionality in Umbraco for a long time.
        var items = await FilterItemsBasedOnAccessAsync(user, pagedChildren.Items);

        var pagedResult = new PagedModel<TContent>
        {
            Items = items,
            Total = pagedChildren.Total,
        };

        return pagedResult;
    }

    private IQuery<TContent>? ParseQueryFilter(string? filter)
    {
        // Adding multiple conditions - considering key (as Guid) & name as filter param
        Guid.TryParse(filter, out Guid filterAsGuid);

        return filter.IsNullOrWhiteSpace()
            ? null
            : _sqlContext.Query<TContent>()
                .Where(c => (c.Name != null && c.Name.Contains(filter)) ||
                            c.Key == filterAsGuid);
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
