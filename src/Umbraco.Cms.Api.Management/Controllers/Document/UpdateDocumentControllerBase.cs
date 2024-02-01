using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public abstract class UpdateDocumentControllerBase : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentEditingService _contentEditingService;

    protected UpdateDocumentControllerBase(IAuthorizationService authorizationService, IContentEditingService contentEditingService)
    {
        _authorizationService = authorizationService;
        _contentEditingService = contentEditingService;
    }

    protected async Task<IActionResult> HandleRequest(Guid id, UpdateDocumentRequestModel requestModel, Func<IContent, Task<IActionResult>> authorizedHandler)
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

        IContent? content = await _contentEditingService.GetAsync(id);
        if (content is null)
        {
            return DocumentNotFound();
        }

        return await authorizedHandler(content);
    }
}
