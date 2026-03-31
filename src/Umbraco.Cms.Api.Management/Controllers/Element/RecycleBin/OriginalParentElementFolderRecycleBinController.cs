using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Querying.RecycleBin;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;

/// <summary>
/// API controller responsible for retrieving the original parent of an element folder in the recycle bin.
/// </summary>
[ApiVersion("1.0")]
public class OriginalParentElementFolderRecycleBinController : ElementRecycleBinControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IElementRecycleBinQueryService _elementRecycleBinQueryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="OriginalParentElementFolderRecycleBinController"/> class.
    /// </summary>
    /// <param name="entityService">Service for retrieving entity data.</param>
    /// <param name="elementPresentationFactory">Factory responsible for creating element presentation models.</param>
    /// <param name="authorizationService">Service used to authorize recycle bin operations.</param>
    /// <param name="elementRecycleBinQueryService">Service for querying the element recycle bin.</param>
    public OriginalParentElementFolderRecycleBinController(
        IEntityService entityService,
        IElementPresentationFactory elementPresentationFactory,
        IAuthorizationService authorizationService,
        IElementRecycleBinQueryService elementRecycleBinQueryService)
        : base(entityService, elementPresentationFactory)
    {
        _authorizationService = authorizationService;
        _elementRecycleBinQueryService = elementRecycleBinQueryService;
    }

    /// <summary>
    /// Gets the original parent location of an element folder before it was moved to the recycle bin.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier of the element folder in the recycle bin.</param>
    /// <returns>An <see cref="IActionResult"/> containing the original parent reference, or null if the parent was root.</returns>
    [HttpGet("folder/{id:guid}/original-parent")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ReferenceByIdModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [EndpointSummary("Gets the original parent of an element folder in the recycle bin.")]
    [EndpointDescription("Gets the original parent location of an element folder before it was moved to the recycle bin.")]
    public async Task<IActionResult> OriginalParentFolder(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.RecycleBin(ActionElementBrowse.ActionLetter),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<IEntitySlim?, RecycleBinQueryResultType> getParentAttempt = await _elementRecycleBinQueryService.GetOriginalParentForContainerAsync(id);
        return getParentAttempt.Success switch
        {
            true when getParentAttempt.Status == RecycleBinQueryResultType.Success
                => Ok(new ReferenceByIdModel(getParentAttempt.Result!.Key)),
            true when getParentAttempt.Status == RecycleBinQueryResultType.ParentIsRoot
                => Ok(null),
            _ => MapRecycleBinQueryAttemptFailure(getParentAttempt.Status, "element folder"),
        };
    }
}
