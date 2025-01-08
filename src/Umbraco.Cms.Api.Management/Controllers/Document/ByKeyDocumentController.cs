using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Security.Authorization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Querying;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class ByKeyDocumentController : DocumentControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IDocumentPresentationFactory _documentPresentationFactory;
    private readonly IContentQueryService _contentQueryService;

    [Obsolete("Scheduled for removal in v17")]
    public ByKeyDocumentController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IDocumentPresentationFactory documentPresentationFactory)
    {
        _authorizationService = authorizationService;
        _documentPresentationFactory = documentPresentationFactory;
        _contentQueryService = StaticServiceProvider.Instance.GetRequiredService<IContentQueryService>();
    }

    // needed for greedy selection until other constructor remains in v17
    [Obsolete("Scheduled for removal in v17")]
    public ByKeyDocumentController(
        IAuthorizationService authorizationService,
        IContentEditingService contentEditingService,
        IDocumentPresentationFactory documentPresentationFactory,
        IContentQueryService contentQueryService)
    {
        _authorizationService = authorizationService;
        _documentPresentationFactory = documentPresentationFactory;
        _contentQueryService = contentQueryService;
    }

    [ActivatorUtilitiesConstructor]
    public ByKeyDocumentController(
        IAuthorizationService authorizationService,
        IDocumentPresentationFactory documentPresentationFactory,
        IContentQueryService contentQueryService)
    {
        _authorizationService = authorizationService;
        _documentPresentationFactory = documentPresentationFactory;
        _contentQueryService = contentQueryService;
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

        var contentWithScheduleAttempt = await _contentQueryService.GetWithSchedulesAsync(id);

        if (contentWithScheduleAttempt.Success == false)
        {
            return ContentQueryOperationStatusResult(contentWithScheduleAttempt.Status);
        }

        DocumentResponseModel model = await _documentPresentationFactory.CreateResponseModelAsync(
            contentWithScheduleAttempt.Result!.Content,
            contentWithScheduleAttempt.Result.Schedules);
        return Ok(model);
    }
}
