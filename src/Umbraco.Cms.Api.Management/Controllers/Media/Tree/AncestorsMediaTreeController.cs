using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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
/// Controller responsible for handling operations related to the ancestors of media items in the media tree.
/// </summary>
[ApiVersion("1.0")]
public class AncestorsMediaTreeController : MediaTreeControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AncestorsMediaTreeController"/> class.
    /// </summary>
    /// <param name="entityService">Service for managing and retrieving entities within the system.</param>
    /// <param name="flagProviders">A collection of providers that supply flags for entities.</param>
    /// <param name="treeFilterService">Service for filtering media tree entities based on user start nodes.</param>
    /// <param name="mediaPresentationFactory">Factory for creating media presentation models.</param>
    [ActivatorUtilitiesConstructor]
    public AncestorsMediaTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IMediaStartNodeTreeFilterService treeFilterService,
        IMediaPresentationFactory mediaPresentationFactory)
        : base(entityService, flagProviders, treeFilterService, mediaPresentationFactory)
    {
    }

    [Obsolete("Please use the non-obsolete constructor. Scheduled for removal in Umbraco 19.")]
    public AncestorsMediaTreeController(
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

    /// <summary>Gets a collection of ancestor media items.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="descendantId">The ID of the descendant media item.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an ActionResult with a collection of MediaTreeItemResponseModel representing the ancestors.</returns>
    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<MediaTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of ancestor media items.")]
    [EndpointDescription("Gets a collection of media items that are ancestors to the provided Id.")]
    public async Task<ActionResult<IEnumerable<MediaTreeItemResponseModel>>> Ancestors(
        CancellationToken cancellationToken,
        Guid descendantId)
        => await GetAncestors(descendantId);
}
