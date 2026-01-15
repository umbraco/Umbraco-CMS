using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Element;

[ApiVersion("1.0")]
public class UnpublishElementController : ElementControllerBase
{
    private readonly IElementPublishingService _elementPublishingService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public UnpublishElementController(
        IElementPublishingService elementPublishingService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _elementPublishingService = elementPublishingService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPut("{id:guid}/unpublish")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unpublish(CancellationToken cancellationToken, Guid id, UnpublishElementRequestModel requestModel)
    {
        Attempt<ContentPublishingOperationStatus> attempt = await _elementPublishingService.UnpublishAsync(
            id,
            requestModel.Cultures,
            CurrentUserKey(_backOfficeSecurityAccessor));
        return attempt.Success
            ? Ok()
            // TODO ELEMENTS: use refactored DocumentPublishingOperationStatusResult from DocumentControllerBase once it's ready
            : BadRequest();
    }
}
