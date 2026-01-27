using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Element.RecycleBin;

[ApiVersion("1.0")]
public class RestoreElementFolderRecycleBinController : ElementRecycleBinControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IElementContainerService _elementContainerService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public RestoreElementFolderRecycleBinController(
        IAuthorizationService authorizationService,
        IElementContainerService elementContainerService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IEntityService entityService,
        IElementPresentationFactory elementPresentationFactory)
        : base(entityService, elementPresentationFactory)
    {
        _authorizationService = authorizationService;
        _elementContainerService = elementContainerService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("folder/{id:guid}/restore")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RestoreFolder(CancellationToken cancellationToken, Guid id, MoveFolderRequestModel moveFolderRequestModel)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ElementPermissionResource.RecycleBin(ActionElementMove.ActionLetter),
            AuthorizationPolicies.ElementPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<EntityContainer?, EntityContainerOperationStatus> result = await _elementContainerService.RestoreAsync(
            id,
            moveFolderRequestModel.Target?.Id,
            CurrentUserKey(_backOfficeSecurityAccessor));

        return result.Success
            ? Ok()
            : OperationStatusResult(result.Status);
    }
}
