using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
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
    /// Initializes a new instance of the <see cref="SiblingsMediaTreeController"/> class, which manages operations related to sibling media items in the media tree.
    /// </summary>
    /// <param name="entityService">Service for accessing and managing entities within the Umbraco CMS.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for entities, used for additional metadata or state.</param>
    /// <param name="treeFilterService">Service for filtering media tree entities based on user start nodes.</param>
    /// <param name="mediaPresentationFactory">Factory for creating media presentation models for API responses.</param>
    [ActivatorUtilitiesConstructor]
    public SiblingsMediaTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IMediaStartNodeTreeFilterService treeFilterService,
        IMediaPresentationFactory mediaPresentationFactory)
        : base(entityService, flagProviders, treeFilterService, mediaPresentationFactory)
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public SiblingsMediaTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        IMediaPresentationFactory mediaPresentationFactory)
        : this(
            entityService,
            flagProviders,
            StaticServiceProvider.Instance.GetRequiredService<IMediaStartNodeTreeFilterService>(),
            mediaPresentationFactory)
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
