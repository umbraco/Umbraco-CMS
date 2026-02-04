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

[ApiVersion("1.0")]
public class OriginalParentElementRecycleBinController : ElementRecycleBinControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IElementRecycleBinQueryService _elementRecycleBinQueryService;

    public OriginalParentElementRecycleBinController(
        IEntityService entityService,
        IElementPresentationFactory elementPresentationFactory,
        IAuthorizationService authorizationService,
        IElementRecycleBinQueryService elementRecycleBinQueryService)
        : base(entityService, elementPresentationFactory)
    {
        _authorizationService = authorizationService;
        _elementRecycleBinQueryService = elementRecycleBinQueryService;
    }

    [HttpGet("{id:guid}/original-parent")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ReferenceByIdModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> OriginalParent(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.RecycleBin(ActionElementBrowse.ActionLetter),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<IEntitySlim?, RecycleBinQueryResultType> getParentAttempt = await _elementRecycleBinQueryService.GetOriginalParentAsync(id);
        return getParentAttempt.Success switch
        {
            true when getParentAttempt.Status == RecycleBinQueryResultType.Success
                => Ok(new ReferenceByIdModel(getParentAttempt.Result!.Key)),
            true when getParentAttempt.Status == RecycleBinQueryResultType.ParentIsRoot
                => Ok(null),
            _ => MapRecycleBinQueryAttemptFailure(getParentAttempt.Status, "element"),
        };
    }
}
