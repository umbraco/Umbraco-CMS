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
/// Controller responsible for handling operations related to sibling media items within the media tree.
/// Provides endpoints for retrieving and managing media items that share the same parent.
/// </summary>
public class SiblingsMediaTreeController : MediaTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SiblingsMediaTreeController"/> class, responsible for handling API requests related to sibling media items in the media tree.
    /// </summary>
    /// <param name="entityService">Service used for entity operations such as retrieval and manipulation.</param>
    /// <param name="userStartNodeEntitiesService">Service that provides access to user start nodes for entity filtering and permissions.</param>
    /// <param name="dataTypeService">Service for managing and retrieving data type definitions.</param>
    /// <param name="appCaches">Provides access to application-level caches for performance optimization.</param>
    /// <param name="backofficeSecurityAccessor">Accessor for backoffice security context, used for authorization and user information.</param>
    /// <param name="mediaPresentationFactory">Factory for creating media presentation models for API responses.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 18.")]
    public SiblingsMediaTreeController(
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
    /// Initializes a new instance of the <see cref="SiblingsMediaTreeController"/> class, which manages operations related to sibling media items in the media tree.
    /// </summary>
    /// <param name="entityService">Service for accessing and managing entities within the Umbraco CMS.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for entities, used for additional metadata or state.</param>
    /// <param name="userStartNodeEntitiesService">Service for resolving the start nodes for users in the media tree.</param>
    /// <param name="dataTypeService">Service for accessing and managing data types in Umbraco.</param>
    /// <param name="appCaches">Provides access to application-level caches for performance optimization.</param>
    /// <param name="backofficeSecurityAccessor">Accessor for back office security context, used for authorization and user information.</param>
    /// <param name="mediaPresentationFactory">Factory for creating media presentation models for API responses.</param>
    [ActivatorUtilitiesConstructor]
    public SiblingsMediaTreeController(
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
    /// Retrieves sibling media items in the media tree for the specified target item.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="target">The unique identifier of the media item whose siblings are to be retrieved.</param>
    /// <param name="before">The number of sibling items to include before the target item.</param>
    /// <param name="after">The number of sibling items to include after the target item.</param>
    /// <param name="dataTypeId">An optional data type identifier to filter the sibling items.</param>
    /// <returns>A task representing the asynchronous operation. The result contains an <see cref="ActionResult{T}"/> with a <see cref="SubsetViewModel{MediaTreeItemResponseModel}"/> representing the sibling media items.</returns>
    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<MediaTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of media tree sibling items.")]
    [EndpointDescription("Gets a collection of media tree items that are siblings of the provided Id.")]
    public async Task<ActionResult<SubsetViewModel<MediaTreeItemResponseModel>>> Siblings(CancellationToken cancellationToken, Guid target, int before, int after, Guid? dataTypeId = null)
    {
        IgnoreUserStartNodesForDataType(dataTypeId);
        return await GetSiblings(target, before, after);
    }
}
