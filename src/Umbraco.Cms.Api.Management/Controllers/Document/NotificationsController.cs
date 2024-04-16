using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class NotificationsController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentEditingService _contentEditingService;
    private readonly IDocumentNotificationPresentationFactory _documentNotificationPresentationFactory;

    public NotificationsController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IDocumentNotificationPresentationFactory documentNotificationPresentationFactory)
    {
        _authorizationService = authorizationService;
        _contentEditingService = contentEditingService;
        _documentNotificationPresentationFactory = documentNotificationPresentationFactory;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}/notifications")]
    [ProducesResponseType(typeof(IEnumerable<DocumentNotificationResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Notifications(CancellationToken cancellationToken, Guid id)
    {
        AuthorizationResult authorizationResult = await _authorizationService.AuthorizeResourceAsync(
            User,
            ContentPermissionResource.WithKeys(ActionBrowse.ActionLetter, id),
            AuthorizationPolicies.ContentPermissionByResource);

        if (!authorizationResult.Succeeded)
        {
            return Forbidden();
        }

        IContent? content = await _contentEditingService.GetAsync(id);
        return content != null
            ? Ok(await _documentNotificationPresentationFactory.CreateNotificationModelsAsync(content))
            : DocumentNotFound();
    }
}
