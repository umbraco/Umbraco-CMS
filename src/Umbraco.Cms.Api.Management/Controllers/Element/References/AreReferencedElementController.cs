using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element.References;

/// <summary>
/// API controller responsible for checking whether elements are referenced by other content.
/// </summary>
[ApiVersion("1.0")]
public class AreReferencedElementController : ElementControllerBase
{
    private readonly ITrackedReferencesService _trackedReferencesService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AreReferencedElementController"/> class.
    /// </summary>
    /// <param name="trackedReferencesService">Service for tracking content references.</param>
    /// <param name="umbracoMapper">Mapper for converting domain models to view models.</param>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 19.")]
    public AreReferencedElementController(
        ITrackedReferencesService trackedReferencesService,
        IUmbracoMapper umbracoMapper)
        : this(
            trackedReferencesService,
            umbracoMapper,
            StaticServiceProvider.Instance.GetRequiredService<IAuthorizationService>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AreReferencedElementController"/> class.
    /// </summary>
    /// <param name="trackedReferencesService">Service for tracking content references.</param>
    /// <param name="umbracoMapper">Mapper for converting domain models to view models.</param>
    /// <param name="authorizationService">The service used to authorize access to the requested items.</param>
    [ActivatorUtilitiesConstructor]
    public AreReferencedElementController(
        ITrackedReferencesService trackedReferencesService,
        IUmbracoMapper umbracoMapper,
        IAuthorizationService authorizationService)
    {
        _trackedReferencesService = trackedReferencesService;
        _umbracoMapper = umbracoMapper;
        _authorizationService = authorizationService;
    }

    /// <summary>
    ///     Gets a paged list of the items used in any kind of relation from selected keys.
    /// </summary>
    /// <remarks>
    ///     Used when bulk deleting elements and bulk unpublishing elements (delete and unpublish on List view).
    ///     This is basically finding children of relations.
    /// </remarks>
    [HttpGet("are-referenced")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<ReferenceByIdModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of items that reference elements.")]
    [EndpointDescription("Gets a paginated collection of items that reference the elements identified by the provided Ids.")]
    public async Task<ActionResult<PagedViewModel<ReferenceByIdModel>>> GetPagedReferencedItems(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids,
        int skip = 0,
        int take = 20)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.WithKeys(ActionElementBrowse.ActionLetter, ids),
            AuthorizationPolicies.ElementPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        PagedModel<Guid> distinctByKeyItemsWithReferencedRelations = await _trackedReferencesService
            .GetPagedKeysWithDependentReferencesAsync(ids, Constants.ObjectTypes.Element, skip, take);

        var pagedViewModel = new PagedViewModel<ReferenceByIdModel>
        {
            Total = distinctByKeyItemsWithReferencedRelations.Total,
            Items = _umbracoMapper.MapEnumerable<Guid, ReferenceByIdModel>(
                distinctByKeyItemsWithReferencedRelations.Items),
        };

        return pagedViewModel;
    }
}
