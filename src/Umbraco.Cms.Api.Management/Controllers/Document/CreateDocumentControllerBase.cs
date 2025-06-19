using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public abstract class CreateDocumentControllerBase : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    protected CreateDocumentControllerBase(IAuthorizationService authorizationService)
        => _authorizationService = authorizationService;

    protected async Task<IActionResult> HandleRequest(CreateDocumentRequestModel requestModel, Func<Task<IActionResult>> authorizedHandler)
    {
        // We intentionally don't pass in cultures here.
        // This is to support the client sending values for all cultures even if the user doesn't have access to the language.
        // Values for unauthorized languages are later ignored in the ContentEditingService.
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionNew.ActionLetter, requestModel.Parent?.Id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (authorizationResult.Succeeded is false)
        {
            return Forbidden();
        }

        return await authorizedHandler();
    }
}
