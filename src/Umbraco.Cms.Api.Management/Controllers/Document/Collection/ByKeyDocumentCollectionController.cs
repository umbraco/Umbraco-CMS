using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
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

namespace Umbraco.Cms.Api.Management.Controllers.Document.Collection;

[ApiVersion("1.0")]
public class ByKeyDocumentCollectionController : DocumentCollectionControllerBase
{
    private readonly IContentEditingService _contentEditingService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IContentTypeService _contentTypeService;
    private readonly ISqlContext _sqlContext;
    private readonly IContentService _contentService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IContentPermissionService _contentPermissionService;
    private readonly IUmbracoMapper _mapper;

    public ByKeyDocumentCollectionController(
        IContentEditingService contentEditingService,
        IDataTypeService dataTypeService,
        IContentTypeService contentTypeService,
        ISqlContext sqlContext,
        IContentService contentService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentPermissionService contentPermissionService,
        IUmbracoMapper mapper)
    {
        _contentEditingService = contentEditingService;
        _dataTypeService = dataTypeService;
        _contentTypeService = contentTypeService;
        _sqlContext = sqlContext;
        _contentService = contentService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _contentPermissionService = contentPermissionService;
        _mapper = mapper;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DocumentCollectionResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(
        Guid id,
        Guid? dataTypeId = null,
        string orderBy = "sortOrder",
        Direction orderDirection = Direction.Ascending,
        string? filter = null, // call query?
        int skip = 0,
        int take = 100)
    {
        // NB: Media id nullable - so if null we work with root
        IContent? content = await _contentEditingService.GetAsync(id);
        if (content == null)
        {
            return CollectionOperationStatusResult(ContentCollectionOperationStatus.ContentNotFound);
        }

        Attempt<ListViewConfiguration?, ContentCollectionOperationStatus> configurationResult = await GetCollectionConfigurationAsync(content.ContentType.Key, dataTypeId); // NB: Media key nullable

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
            "da-DK", // TODO: Might be a problem that we are not passing in culture? (not needed for media?)
            isCustomField: orderByCustomField);

        IUser user = CurrentUser(_backOfficeSecurityAccessor);
        PagedModel<IContent> result = await GetAllowedCollectionItems(user, content.Id, ordering, skip, take, filter); // NB: Media nullable, pass in system.root

        List<DocumentCollectionResponseModel> collectionResponseModels =
            _mapper.MapEnumerable<IContent, DocumentCollectionResponseModel>(result.Items, context =>
            {
                context.SetIncludedProperties(collectionPropertyAliases.ToArray());
            });

        var pageViewModel = new PagedViewModel<DocumentCollectionResponseModel>
        {
            Total = result.Total,
            Items = collectionResponseModels
        };

        return Ok(pageViewModel);
    }

    private async Task<Attempt<ListViewConfiguration?, ContentCollectionOperationStatus>> GetCollectionConfigurationAsync(Guid contentTypeKey, Guid? dataTypeKey) // NB: Media key nullable
    {
        Attempt<ListViewConfiguration?, ContentCollectionOperationStatus> collectionConfigurationAttempt;

        if (dataTypeKey.HasValue) // NB: Additional condition as Media key nullable
        {
            collectionConfigurationAttempt = await GetCollectionConfigurationFromDataTypeAsync(dataTypeKey.Value, contentTypeKey);
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

        IContentType? contentType = await _contentTypeService.GetAsync(contentTypeKey);
        if (contentType is null)
        {
            return Attempt.FailWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.ContentTypeNotFound, null);
        }

        // Check if the data type collection is a content property
        return contentType.CompositionPropertyTypes.Any(pt => pt.DataTypeKey == dataTypeKey)
            ? Attempt.SucceedWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.Success, listViewConfiguration)
            : Attempt.FailWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.DataTypeNotContentProperty, null);
    }

    private async Task<Attempt<ListViewConfiguration?, ContentCollectionOperationStatus>> GetCollectionConfigurationFromContentTypeAsync(Guid contentTypeKey) // NB: Media key nullable
    {
        // NB: additional checks for Media key nullable
        IContentType? contentType = await _contentTypeService.GetAsync(contentTypeKey);
        if (contentType is null)
        {
            return Attempt.FailWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.ContentTypeNotFound, null);
        }

        if (contentType.IsContainer == false)
        {
            return Attempt.FailWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.ContentNotCollection, null);
        }

        IDataType? currentCollectionDataType = await GetConfiguredCollectionDataTypeAsync(contentType.Alias);

        return currentCollectionDataType?.ConfigurationObject is ListViewConfiguration collectionConfiguration
            ? Attempt.SucceedWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.Success, collectionConfiguration)
            : Attempt.FailWithStatus<ListViewConfiguration?, ContentCollectionOperationStatus>(ContentCollectionOperationStatus.CollectionNotFound, null);
    }

    // TODO: How to get the ListViewConfiguration from doc type?
    private async Task<IDataType?> GetConfiguredCollectionDataTypeAsync(string? listViewSuffix)
    {
        var defaultContentCollectionDataType = await _dataTypeService.GetAsync(Constants.DataTypes.Guids.ListViewContentGuid); // NB: different for media - ListViewMediaGuid

        if (string.IsNullOrEmpty(listViewSuffix))
        {
            return defaultContentCollectionDataType;
        }

        // This is a hack based on legacy (same thing can be seen in ListViewContentAppFactory) - we cannot infer the list view associated with a content type otherwise.
        // We can use the fact that when a custom list view is removed as the content type list view configuration, the corresponding list view data type gets deleted.
        var customCollectionName = Constants.Conventions.DataTypes.ListViewPrefix + listViewSuffix;

        return _dataTypeService.GetDataType(customCollectionName) ?? defaultContentCollectionDataType;
    }

    // private async Task<IDataType?> GetCollectionDataTypeFromContentTypeAsync(IContentType contentType) // NB: Media key nullable
    // {
    //     // This is a hack based on legacy (same thing can be seen in ListViewContentAppFactory) - we cannot infer the list view associated with a content type otherwise.
    //     // We can use the fact that when a custom list view is removed as the content type list view configuration, the corresponding list view data type gets deleted.
    //     var customCollectionName = Constants.Conventions.DataTypes.ListViewPrefix + contentType.Alias; // NB: different for media - mediaType.Name
    //     var defaultContentCollectionDataType = await _dataTypeService.GetAsync(Constants.DataTypes.Guids.ListViewContentGuid); // NB: different for media - ListViewMediaGuid
    //
    //     return _dataTypeService.GetDataType(customCollectionName) ?? defaultContentCollectionDataType;
    // }

    // private async Task<IDataType?> GetDefaultListViewForEntity(object entity)
    // {
    //     Guid defaultListViewGuid;
    //     switch (entity)
    //     {
    //         case IContent _:
    //             defaultListViewGuid = Constants.DataTypes.Guids.ListViewContentGuid;
    //             break;
    //         case IMedia _:
    //             defaultListViewGuid = Constants.DataTypes.Guids.ListViewMediaGuid;
    //             break;
    //         case IMember _:
    //             defaultListViewGuid = Constants.DataTypes.Guids.ListViewMembersGuid;
    //             break;
    //         default:
    //             throw new ArgumentException(
    //                 $"Entity type does not have a default list view data type: {entity.GetType().FullName}",
    //                 nameof(entity));
    //     }
    //
    //     return await _dataTypeService.GetAsync(defaultListViewGuid);
    // }

    private async Task<PagedModel<IContent>> GetAllowedCollectionItems(IUser user, int contentId, Ordering ordering, int skip, int take, string? filter = null)
    {
        PaginationHelper.ConvertSkipTakeToPaging(skip, take, out var pageNumber, out var pageSize);

        var queryFilter = ParseQueryFilter(filter);

        IEnumerable<IContent> children = _contentService.GetPagedChildren( // NB: different for media
            contentId,
            pageNumber, //pageNumber - 1,
            pageSize,
            out var total,
            queryFilter,
            ordering);

        // Filtering out child nodes after getting a paged result is an active choice here, even though the pagination might get off.
        // This has been the case with this functionality in Umbraco for a long time.
        var items = await FilterItemsBasedOnAccess(children, user);

        var pagedResult = new PagedModel<IContent>
        {
            Items = items,
            Total = total
        };

        return pagedResult;
    }

    private IQuery<IContent>? ParseQueryFilter(string? filter)
    {
        // Adding multiple conditions - considering id (as int), key (as Guid) & name as filter param
        int.TryParse(filter, out int filterAsIntId);
        Guid.TryParse(filter, out Guid filterAsGuid);

        return filter.IsNullOrWhiteSpace()
            ? null
            : _sqlContext.Query<IContent>()
                .Where(c => (c.Name != null && c.Name.Contains(filter)) ||
                            c.Id == filterAsIntId ||
                            c.Key == filterAsGuid);
    }

    // We can use an authorizer here, as it already handles all the necessary checks for this filtering.
    // However, we cannot pass in all the items; we want only the ones that comply, as opposed to
    // a general response whether the user has access to all nodes.
    // TODO: Optimize the way we filter out only the nodes the user is allowed to see
    private async Task<IEnumerable<IContent>> FilterItemsBasedOnAccess(IEnumerable<IContent> items, IUser user)
    {
        var filteredItems = new List<IContent>();

        foreach (IContent item in items)
        {
            // TODO: Consider if it is better to use IContentPermissionAuthorizer here as people will be able to apply their external authorization
            ContentAuthorizationStatus accessStatus = await _contentPermissionService.AuthorizeAccessAsync(
                user,
                item.Key,
                ActionBrowse.ActionLetter); // NB: not necessary for media

            if (accessStatus == ContentAuthorizationStatus.Success)
            {
                filteredItems.Add(item);
            }

            // var isAuthorized = await _contentPermissionAuthorizer.IsAuthorizedAsync(
            //
            //     item.Key,
            //     ActionBrowse.ActionLetter);
            //
            // if (isAuthorized)
            // {
            //     filteredItems.Add(item);
            // }
        }

        return filteredItems;
    }
}
