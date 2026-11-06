using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Media.References;

/// <summary>
/// Controller responsible for managing media items that are referenced as descendants.
/// </summary>
[ApiVersion("1.0")]
public class ReferencedDescendantsMediaController : MediaControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesSkipTakeService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IAuthorizationService _authorizationService;
    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencedDescendantsMediaController"/> class.
    /// </summary>
    /// <param name="trackedReferencesSkipTakeService">Service for retrieving tracked references with skip/take pagination.</param>
    /// <param name="umbracoMapper">The mapper used to map Umbraco domain models to API models.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public ReferencedDescendantsMediaController(
        ITrackedReferencesService trackedReferencesSkipTakeService,
        IUmbracoMapper umbracoMapper)
        : this(
            trackedReferencesSkipTakeService,
            umbracoMapper,
            StaticServiceProvider.Instance.GetRequiredService<IAuthorizationService>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferencedDescendantsMediaController"/> class.
    /// </summary>
    /// <param name="trackedReferencesSkipTakeService">Service for retrieving tracked references with skip/take pagination.</param>
    /// <param name="umbracoMapper">The mapper used to map Umbraco domain models to API models.</param>
    /// <param name="authorizationService">The service used to authorize access to the requested items.</param>
    [ActivatorUtilitiesConstructor]
    public ReferencedDescendantsMediaController(
        ITrackedReferencesService trackedReferencesSkipTakeService,
        IUmbracoMapper umbracoMapper,
        IAuthorizationService authorizationService)
    {
        _trackedReferencesSkipTakeService = trackedReferencesSkipTakeService;
        _umbracoMapper = umbracoMapper;
        _authorizationService = authorizationService;
    }

    [Obsolete("Use the ReferencedDescendants2 action method instead. Scheduled for removal in Umbraco 19, when ReferencedDescendants2 will be renamed back to ReferencedDescendants.")]
    [NonAction]
    public async Task<ActionResult<PagedViewModel<ReferenceByIdModel>>> ReferencedDescendants(
    CancellationToken cancellationToken,
    Guid id,
    int skip = 0,
    int take = 20)
    {
        PagedModel<RelationItemModel> relationItems = await _trackedReferencesSkipTakeService.GetPagedDescendantsInReferencesAsync(id, skip, take, true);
        var pagedViewModel = new PagedViewModel<ReferenceByIdModel>
        {
            Total = relationItems.Total,
            Items = _umbracoMapper.MapEnumerable<RelationItemModel, ReferenceByIdModel>(relationItems.Items),
        };

        return pagedViewModel;
    }

    /// <summary>
    ///     Gets a paginated list of descendant media items of the specified item that are referenced in any kind of relation.
    /// </summary>
    /// <remarks>
    ///     Used when deleting or unpublishing a single media item to check if it has any descendant items that are referenced in any relation.
    ///     This operation finds descendant items that are children in relations.
    /// </remarks>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the media item whose referenced descendants are to be retrieved.</param>
    /// <param name="skip">The number of items to skip before starting to collect the result set (used for pagination).</param>
    /// <param name="take">The maximum number of items to return (used for pagination).</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/> with a paginated list of referenced descendant media items.</returns>
    [HttpGet("{id:guid}/referenced-descendants")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ReferenceByIdModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets media descendants that are referenced.")]
    [EndpointDescription("Gets a paginated collection of descendant media items that are referenced by other content.")]
    public async Task<IActionResult> ReferencedDescendants2(
        CancellationToken cancellationToken,
        Guid id,
        int skip = 0,
        int take = 20)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            MediaPermissionResource.WithKeys(id),
            AuthorizationPolicies.MediaPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus> relationItemsAttempt = await _trackedReferencesSkipTakeService.GetPagedDescendantsInReferencesAsync(id, UmbracoObjectTypes.Media, skip, take, true);

        if (relationItemsAttempt.Success is false)
        {
            return GetReferencesOperationStatusResult(relationItemsAttempt.Status);
        }

        var pagedViewModel = new PagedViewModel<ReferenceByIdModel>
        {
            Total = relationItemsAttempt.Result.Total,
            Items = _umbracoMapper.MapEnumerable<RelationItemModel, ReferenceByIdModel>(relationItemsAttempt.Result.Items),
        };

        return Ok(pagedViewModel);
    }
}
