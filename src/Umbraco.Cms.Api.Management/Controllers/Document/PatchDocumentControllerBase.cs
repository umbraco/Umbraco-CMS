using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.OperationStatus;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public abstract class PatchDocumentControllerBase : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    protected PatchDocumentControllerBase(IAuthorizationService authorizationService)
        => _authorizationService = authorizationService;

    protected async Task<IActionResult> HandleRequest(Guid id, PatchDocumentRequestModel requestModel, Func<Task<IActionResult>> authorizedHandler)
    {
        // We intentionally don't pass in cultures here.
        // This is to support the client sending values for all cultures even if the user doesn't have access to the language.
        // Values for unauthorized languages are later ignored in the ContentEditingService.
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionUpdate.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        return await authorizedHandler();
    }

    /// <summary>
    /// Maps ContentPatchingOperationStatus to appropriate HTTP responses for PATCH operations.
    /// </summary>
    protected IActionResult ContentPatchingOperationStatusResult(ContentPatchingOperationStatus status)
        => OperationStatusResult(status, problemDetailsBuilder => status switch
        {
            ContentPatchingOperationStatus.InvalidOperation => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid operation")
                .WithDetail("One or more PATCH operations were invalid. Check operation structure, JSONPath syntax, and operation types.")
                .Build()),
            ContentPatchingOperationStatus.InvalidCulture => BadRequest(problemDetailsBuilder
                .WithTitle("Invalid culture")
                .WithDetail("One or more cultures specified in operation paths are not valid or not configured.")
                .Build()),
            ContentPatchingOperationStatus.NotFound => NotFound(problemDetailsBuilder
                .WithTitle("The document could not be found")
                .Build()),
            ContentPatchingOperationStatus.ContentTypeNotFound => NotFound(problemDetailsBuilder
                .WithTitle("The document's content type could not be found")
                .Build()),
            ContentPatchingOperationStatus.PropertyTypeNotFound => UnprocessableEntity(problemDetailsBuilder
                .WithTitle("Property type not found")
                .WithDetail("One or more specified properties do not exist on the content type.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, problemDetailsBuilder
                .WithTitle("Unknown error")
                .WithDetail("An unexpected error occurred during the PATCH operation.")
                .Build())
        });
}
