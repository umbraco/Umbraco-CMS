using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Security.Authorization.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class ByKeyDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentEditingService _contentEditingService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;
    private readonly IContentService _contentService;

    [Obsolete("Scheduled for removal in v17")]
    public ByKeyDocumentController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IDocumentPresentationFactory documentPresentationFactory)
    {
        _authorizationService = authorizationService;
        _contentEditingService = contentEditingService;
        _documentPresentationFactory = documentPresentationFactory;
        _contentService = StaticServiceProvider.Instance.GetRequiredService<IContentService>();
    }

    [ActivatorUtilitiesConstructor]
    public ByKeyDocumentController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IDocumentPresentationFactory documentPresentationFactory,
        IContentService contentService)
    {
        _authorizationService = authorizationService;
        _contentEditingService = contentEditingService;
        _documentPresentationFactory = documentPresentationFactory;
        _contentService = contentService;
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DocumentResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(CancellationToken cancellationToken, Guid id)
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
        if (content == null)
        {
            return DocumentNotFound();
        }

        ContentScheduleCollection schedule = _contentService.GetContentScheduleByContentId(content.Id);

        DocumentResponseModel model = await _documentPresentationFactory.CreateResponseModelAsync(content, schedule);
        return Ok(model);
    }
}
