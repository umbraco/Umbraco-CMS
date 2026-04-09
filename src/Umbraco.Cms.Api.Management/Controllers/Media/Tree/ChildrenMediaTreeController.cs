using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Media.Tree;

/// <summary>
/// Controller responsible for handling operations related to the child nodes of media items in the media tree.
/// </summary>
[ApiVersion("1.0")]
public class ChildrenMediaTreeController : MediaTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenMediaTreeController"/> class, responsible for handling API requests related to child media items in the media tree.
    /// </summary>
    /// <param name="entityService">Service for accessing and managing entities within the Umbraco CMS.</param>
    /// <param name="userStartNodeEntitiesService">Service for resolving user-specific start nodes for entities.</param>
    /// <param name="dataTypeService">Service for managing data types used by media items.</param>
    /// <param name="appCaches">Provides access to application-level caches for performance optimization.</param>
    /// <param name="backofficeSecurityAccessor">Accessor for backoffice security context, used for authorization and user information.</param>
    /// <param name="mediaPresentationFactory">Factory for creating presentation models for media items.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public ChildrenMediaTreeController(
        IEntityService entityService,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IMediaPresentationFactory mediaPresentationFactory)
        : base(entityService, userStartNodeEntitiesService, dataTypeService, appCaches, backofficeSecurityAccessor, mediaPresentationFactory)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChildrenMediaTreeController"/> class, responsible for handling API requests related to retrieving child media items in the media tree.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities within the Umbraco system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for entities, used for additional metadata or state.</param>
    /// <param name="userStartNodeEntitiesService">Service for resolving the start nodes for users, determining their access within the media tree.</param>
    /// <param name="dataTypeService">Service for accessing and managing data types in Umbraco.</param>
    /// <param name="appCaches">Provides access to application-level caches for performance optimization.</param>
    /// <param name="backofficeSecurityAccessor">Accessor for backoffice security context, used for authorization and user information.</param>
    /// <param name="mediaPresentationFactory">Factory for creating presentation models for media entities.</param>
    [ActivatorUtilitiesConstructor]
    public ChildrenMediaTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IMediaPresentationFactory mediaPresentationFactory)
        : base(entityService, flagProviders, userStartNodeEntitiesService, dataTypeService, appCaches, backofficeSecurityAccessor, mediaPresentationFactory)
    {
    }

    /// <summary>
    /// Retrieves a paginated list of media tree items that are direct children of the specified parent media item.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="parentId">The unique identifier of the parent media item whose children are to be retrieved.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (for pagination).</param>
    /// <param name="take">The maximum number of items to return (for pagination).</param>
    /// <param name="dataTypeId">An optional data type identifier to filter the media items.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="ActionResult{T}"/> with a paged view model of <see cref="MediaTreeItemResponseModel"/> items.</returns>
    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<MediaTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of media tree child items.")]
    [EndpointDescription("Gets a paginated collection of media tree items that are children of the provided parent Id.")]
    public async Task<ActionResult<PagedViewModel<MediaTreeItemResponseModel>>> Children(
        CancellationToken cancellationToken,
        Guid parentId,
        int skip = 0,
        int take = 100,
        Guid? dataTypeId = null)
    {
        IgnoreUserStartNodesForDataType(dataTypeId);
        return await GetChildren(parentId, skip, take);
    }
}
