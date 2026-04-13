using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.Services.Flags;
using Umbraco.Cms.Api.Management.Services.PermissionFilter;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Element.Tree;

/// <summary>
/// API controller responsible for retrieving ancestor element tree items.
/// </summary>
[ApiVersion("1.0")]
public class AncestorsElementTreeController : ElementTreeControllerBase
{
    /// <inheritdoc />
    public AncestorsElementTreeController(
        IEntityService entityService,
        FlagProviderCollection flagProviders,
        IUserStartNodeEntitiesService userStartNodeEntitiesService,
        IDataTypeService dataTypeService,
        AppCaches appCaches,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IElementPresentationFactory elementPresentationFactory,
        IElementPermissionFilterService elementPermissionFilterService)
        : base(entityService, flagProviders, userStartNodeEntitiesService, dataTypeService, appCaches, backOfficeSecurityAccessor, elementPresentationFactory, elementPermissionFilterService)
    {
    }

    /// <summary>
    /// Gets a collection of ancestor element tree items for the specified descendant.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="descendantId">The unique identifier of the descendant element.</param>
    /// <returns>A collection of ancestor element tree items.</returns>
    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ElementTreeItemResponseModel>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of ancestor element items.")]
    [EndpointDescription("Gets a collection of element items that are ancestors to the provided Id.")]
    public async Task<ActionResult<IEnumerable<ElementTreeItemResponseModel>>> Ancestors(CancellationToken cancellationToken, Guid descendantId)
        => await GetAncestors(descendantId);
}
