using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

[ApiVersion("1.0")]
public class UpdateNotificationsController : DocumentControllerBase
{
    private readonly IContentEditingService _contentEditingService;
    private readonly INotificationService _notificationService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UpdateNotificationsController(IContentEditingService contentEditingService, INotificationService notificationService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _contentEditingService = contentEditingService;
        _notificationService = notificationService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [MapToApiVersion("1.0")]
    [HttpPut("{id:guid}/notifications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateNotifications(CancellationToken cancellationToken, Guid id, UpdateDocumentNotificationsRequestModel updateModel)
    {
        IContent? content = await _contentEditingService.GetAsync(id);
        if (content == null)
        {
            return DocumentNotFound();
        }

        _notificationService.SetNotifications(_backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser, content, updateModel.SubscribedActionIds);
        return Ok();
    }
}
