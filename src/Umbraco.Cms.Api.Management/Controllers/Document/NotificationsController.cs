using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class NotificationsController : DocumentControllerBase
{
    private readonly IContentEditingService _contentEditingService;
    private readonly IDocumentNotificationPresentationFactory _documentNotificationPresentationFactory;

    public NotificationsController(IContentEditingService contentEditingService, IDocumentNotificationPresentationFactory documentNotificationPresentationFactory)
    {
        _contentEditingService = contentEditingService;
        _documentNotificationPresentationFactory = documentNotificationPresentationFactory;
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}/notifications")]
    [ProducesResponseType(typeof(IEnumerable<DocumentNotificationResponseModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Notifications(Guid id)
    {
        IContent? content = await _contentEditingService.GetAsync(id);
        return content != null
            ? Ok(await _documentNotificationPresentationFactory.CreateNotificationModelsAsync(content))
            : DocumentNotFound();
    }
}
