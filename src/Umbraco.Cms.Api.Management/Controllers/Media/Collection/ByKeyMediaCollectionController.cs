using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Media.Collection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Collection;

[ApiVersion("1.0")]
public class ByKeyMediaCollectionController : MediaCollectionControllerBase
{
    private readonly IMediaEditingService _mediaEditingService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly ISqlContext _sqlContext;
    private readonly IMediaService _mediaService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IMediaPermissionService _mediaPermissionService;
    private readonly IUmbracoMapper _mapper;

    public ByKeyMediaCollectionController(
        IMediaEditingService mediaEditingService,
        IDataTypeService dataTypeService,
        IMediaTypeService mediaTypeService,
        ISqlContext sqlContext,
        IMediaService mediaService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IMediaPermissionService mediaPermissionService,
        IUmbracoMapper mapper)
    {
        _mediaEditingService = mediaEditingService;
        _dataTypeService = dataTypeService;
        _mediaTypeService = mediaTypeService;
        _sqlContext = sqlContext;
        _mediaService = mediaService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _mediaPermissionService = mediaPermissionService;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MediaCollectionResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(
        Guid? id,
        Guid? dataTypeId = null,
        string orderBy = "sortOrder",
        Direction orderDirection = Direction.Ascending,
        string? filter = null, // call query?
        int skip = 0,
        int take = 100)
    {
        IMedia? media = null;

        if (id.HasValue)
        {
            media = await _mediaEditingService.GetAsync(id.Value);

            if (media == null)
            {
                return CollectionOperationStatusResult(ContentCollectionOperationStatus.ContentNotFound);
            }
        }

        Attempt<ListViewConfiguration?, ContentCollectionOperationStatus> configurationResult = await GetCollectionConfigurationAsync(media?.ContentType.Key, dataTypeId);

        if (configurationResult.Success == false)
        {
            return CollectionOperationStatusResult(configurationResult.Status);
        }

        var collectionProperties = configurationResult.Result?.IncludeProperties;

        if (collectionProperties == null || collectionProperties.Length == 0)
        {
            return CollectionOperationStatusResult(ContentCollectionOperationStatus.MissingPropertiesInCollectionConfiguration);
        }

        var collectionPropertyAliases = collectionProperties
            .Select(p => p.Alias)
            .WhereNotNull();

        if (collectionPropertyAliases.Contains(orderBy) == false && orderBy.InvariantEquals("name") == false)
        {
            return CollectionOperationStatusResult(ContentCollectionOperationStatus.OrderByNotPartOfCollectionConfiguration);
        }

        var orderByCustomField = collectionProperties
            .Any(p => p.Alias == orderBy && p.IsSystem == 0);

        var ordering = Ordering.By(
            orderBy,
            orderDirection,
            isCustomField: orderByCustomField);

        IUser user = CurrentUser(_backOfficeSecurityAccessor);
        PagedModel<IMedia> result = await GetAllowedCollectionItems(user, media?.Id ?? Constants.System.Root, ordering, skip, take, filter);

        List<MediaCollectionResponseModel> collectionResponseModels =
            _mapper.MapEnumerable<IMedia, MediaCollectionResponseModel>(result.Items, context =>
            {
                context.SetIncludedProperties(collectionPropertyAliases.ToArray());
            });

        var pageViewModel = new PagedViewModel<MediaCollectionResponseModel>
        {
            Total = result.Total,
            Items = collectionResponseModels
        };

        return Ok(pageViewModel);
    }

    private async Task<Attempt<ListViewConfiguration?, ContentCollectionOperationStatus>> GetCollectionConfigurationAsync(Guid? contentTypeKey, Guid? dataTypeKey)
    {
        Attempt<ListViewConfiguration?, ContentCollectionOperationStatus> collectionConfigurationAttempt;

        if (dataTypeKey.HasValue && contentTypeKey.HasValue)
        {
            collectionConfigurationAttempt = await GetCollectionConfigurationFromDataTypeAsync(dataTypeKey.Value, contentTypeKey.Value);
        }
        else
        {
            // Find collection configuration from content type
            collectionConfigurationAttempt = await GetCollectionConfigurationFromContentTypeAsync(contentTypeKey);
        }

        return collectionConfigurationAttempt.Success
            ? Attempt.SucceedWithStatus(ContentCollectionOperationStatus.Success, collectionConfigurationAttempt.Result)
            : collectionConfigurationAttempt;
    }

    private async Task<Attempt<ListViewConfiguration?, ContentCollectionOperationStatus>> GetCollectionConfigurationFromDataTypeAsync(Guid dataTypeKey, Guid contentTypeKey)
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

        IMediaType? mediaType = await _mediaTypeService.GetAsync(contentTypeKey);
        if (mediaType is null)
        {
            return Attempt.FailWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.ContentTypeNotFound, null);
        }

        // Check if the data type collection is a media property
        return mediaType.CompositionPropertyTypes.Any(pt => pt.DataTypeKey == dataTypeKey)
            ? Attempt.SucceedWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.Success, listViewConfiguration)
            : Attempt.FailWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.DataTypeNotContentProperty, null);
    }

    private async Task<Attempt<ListViewConfiguration?, ContentCollectionOperationStatus>> GetCollectionConfigurationFromContentTypeAsync(Guid? contentTypeKey)
    {
        IMediaType? mediaType = null;

        if (contentTypeKey.HasValue)
        {
            mediaType = await _mediaTypeService.GetAsync(contentTypeKey.Value);
            if (mediaType is null)
            {
                return Attempt.FailWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.ContentTypeNotFound, null);
            }

            if (mediaType.IsContainer == false)
            {
                return Attempt.FailWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.ContentNotCollection, null);
            }
        }

        IDataType? currentCollectionDataType = await GetConfiguredCollectionDataTypeAsync(mediaType?.Name);

        return currentCollectionDataType?.ConfigurationObject is ListViewConfiguration collectionConfiguration
            ? Attempt.SucceedWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.Success, collectionConfiguration)
            : Attempt.FailWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.CollectionNotFound, null);
    }

    // TODO: How to get the ListViewConfiguration from doc type?
    private async Task<IDataType?> GetConfiguredCollectionDataTypeAsync(string? listViewSuffix)
    {
        var defaultMediaCollectionDataType = await _dataTypeService.GetAsync(Constants.DataTypes.Guids.ListViewMediaGuid); // NB: different for media - ListViewMediaGuid

        if (string.IsNullOrEmpty(listViewSuffix))
        {
            return defaultMediaCollectionDataType;
        }

        // This is a hack based on legacy (same thing can be seen in ListViewContentAppFactory) - we cannot infer the list view associated with a content type otherwise.
        // We can use the fact that when a custom list view is removed as the media type list view configuration, the corresponding list view data type gets deleted.
        var customCollectionName = Constants.Conventions.DataTypes.ListViewPrefix + listViewSuffix;

        return _dataTypeService.GetDataType(customCollectionName) ?? defaultMediaCollectionDataType;
    }

    // private async Task<IDataType?> GetCollectionDataTypeFromMediaTypeAsync(IMediaType? mediaType)
    // {
    //     // This is a hack based on legacy (same thing can be seen in ListViewContentAppFactory) - we cannot infer the list view associated with a content type otherwise.
    //     // We can use the fact that when a custom list view is removed as the media type list view configuration, the corresponding list view data type gets deleted.
    //     var customCollectionName = mediaType == null ? string.Empty : Constants.Conventions.DataTypes.ListViewPrefix + mediaType.Name; // NB: different for media
    //     var defaultMediaCollectionDataType = await _dataTypeService.GetAsync(Constants.DataTypes.Guids.ListViewMediaGuid);
    //
    //     return _dataTypeService.GetDataType(customCollectionName) ?? defaultMediaCollectionDataType;
    // }

    private async Task<PagedModel<IMedia>> GetAllowedCollectionItems(IUser user, int contentId, Ordering ordering, int skip, int take, string? filter = null)
    {
        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        var queryFilter = ParseQueryFilter(filter);

        IEnumerable<IMedia> children = _mediaService.GetPagedChildren( // NB: different for media
            contentId,
            pageNumber, //pageNumber - 1,
            pageSize,
            out var total,
            queryFilter,
            ordering);

        // Filtering out child nodes after getting a paged result is an active choice here, even though the pagination might get off.
        // This has been the case with this functionality in Umbraco for a long time.
        var items = await FilterItemsBasedOnAccess(children, user);

        var pagedResult = new PagedModel<IMedia>
        {
            Items = items,
            Total = total
        };

        return pagedResult;
    }

    private IQuery<IMedia>? ParseQueryFilter(string? filter)
    {
        // Adding multiple conditions - considering key (as Guid) & name as filter param
        Guid.TryParse(filter, out Guid filterAsGuid);

        return filter.IsNullOrWhiteSpace()
            ? null
            : _sqlContext.Query<IMedia>()
                .Where(c => (c.Name != null && c.Name.Contains(filter)) ||
                            c.Key == filterAsGuid);
    }

    // We can use an authorizer here, as it already handles all the necessary checks for this filtering.
    // However, we cannot pass in all the items; we want only the ones that comply, as opposed to
    // a general response whether the user has access to all nodes.
    // TODO: Optimize the way we filter out only the nodes the user is allowed to see
    private async Task<IEnumerable<IMedia>> FilterItemsBasedOnAccess(IEnumerable<IMedia> items, IUser user)
    {
        var filteredItems = new List<IMedia>();

        foreach (IMedia item in items)
        {
            // TODO: Consider if it is better to use IMediaPermissionAuthorizer here as people will be able to apply their external authorization
            MediaAuthorizationStatus accessStatus = await _mediaPermissionService.AuthorizeAccessAsync(
                user,
                item.Key);

            if (accessStatus == MediaAuthorizationStatus.Success)
            {
                filteredItems.Add(item);
            }

            // var isAuthorized = await _mediaPermissionAuthorizer.IsAuthorizedAsync(
            //
            //     item.Key);
            //
            // if (isAuthorized)
            // {
            //     filteredItems.Add(item);
            // }
        }

        return filteredItems;
    }
}
