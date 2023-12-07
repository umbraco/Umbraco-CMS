using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Api.Management.ViewModels.PublicAccess;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public class CreatePublicAccessDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IPublicAccessPresentationFactory _publicAccessPresentationFactory;
    private readonly IPublicAccessService _publicAccessService;

    public CreatePublicAccessDocumentController(
        IAuthorizationService authorizationService,
        IPublicAccessPresentationFactory publicAccessPresentationFactory,
        IPublicAccessService publicAccessService)
    {
        _authorizationService = authorizationService;
        _publicAccessPresentationFactory = publicAccessPresentationFactory;
        _publicAccessService = publicAccessService;
    }

    [MapToApiVersion("1.0")]
    [HttpPost("{id:guid}/public-access")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(Guid id, PublicAccessRequestModel publicAccessRequestModel)
    {
        var resource = new ContentPermissionResource(id, ActionProtect.ActionLetter);
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(User, resource,
            $"New{AuthorizationPolicies.ContentPermissionByResource}");

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        PublicAccessEntrySlim publicAccessEntrySlim = _publicAccessPresentationFactory.CreatePublicAccessEntrySlim(publicAccessRequestModel, id);

        Attempt<PublicAccessEntry?, PublicAccessOperationStatus> saveAttempt = await _publicAccessService.CreateAsync(publicAccessEntrySlim);

        return saveAttempt.Success
            ? CreatedAtAction<GetPublicAccessDocumentController>(controller => nameof(controller.GetPublicAccess), saveAttempt.Result!.Key)
            : PublicAccessOperationStatusResult(saveAttempt.Status);
    }
}
