using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.PublicAccess;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class GetPublicAccessDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IPublicAccessService _publicAccessService;
    private readonly IPublicAccessPresentationFactory _publicAccessPresentationFactory;

    public GetPublicAccessDocumentController(
        IAuthorizationService authorizationService,
        IPublicAccessService publicAccessService,
        IPublicAccessPresentationFactory publicAccessPresentationFactory)
    {
        _authorizationService = authorizationService;
        _publicAccessService = publicAccessService;
        _publicAccessPresentationFactory = publicAccessPresentationFactory;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}/public-access")]
    [ProducesResponseType(typeof(PublicAccessResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPublicAccess(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionProtect.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        Attempt<PublicAccessEntry?, PublicAccessOperationStatus> accessAttempt =
            await _publicAccessService.GetEntryByContentKeyWithoutAncestorsAsync(id);

        if (accessAttempt.Success is false || accessAttempt.Result is null)
        {
            return PublicAccessOperationStatusResult(accessAttempt.Status);
        }

        Attempt<PublicAccessResponseModel?, PublicAccessOperationStatus> responseModelAttempt =
            _publicAccessPresentationFactory.CreatePublicAccessResponseModel(accessAttempt.Result);

        if (responseModelAttempt.Success is false)
        {
            return PublicAccessOperationStatusResult(responseModelAttempt.Status);
        }

        return Ok(responseModelAttempt.Result);
    }
}
