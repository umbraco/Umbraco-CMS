using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public abstract class UpdateDocumentControllerBase : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;

    protected UpdateDocumentControllerBase(IAuthorizationService authorizationService)
        => _authorizationService = authorizationService;

    protected async Task<IActionResult> HandleRequest(Guid id, UpdateDocumentRequestModel requestModel, Func<Task<IActionResult>> authorizedHandler)
    {
        IEnumerable<string> cultures = requestModel.Variants
            .Where(v => v.Culture is not null)
            .Select(v => v.Culture!);
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionUpdate.ActionLetter, id, cultures),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        return await authorizedHandler();
    }
}
