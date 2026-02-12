using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Preview;

[ApiVersion("1.0")]
[Obsolete("Do not use this. Preview state is initiated implicitly by the preview URL generation. Scheduled for removal in Umbraco 18.")]
public class EnterPreviewController : PreviewControllerBase
{
    private readonly IPreviewService _previewService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    public EnterPreviewController(IPreviewService previewService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _previewService = previewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Enters preview mode.")]
    [EndpointDescription("Enters preview mode for the current user session, allowing viewing of unpublished content.")]
    public async Task<IActionResult> Enter(CancellationToken cancellationToken)
    {
        return await _previewService.TryEnterPreviewAsync(CurrentUser(_backOfficeSecurityAccessor))
            ? Ok()
            : StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Could not enter preview",
                Detail = "Something unexpected went wrong trying to activate preview mode for the current user",
                Status = StatusCodes.Status500InternalServerError,
                Type = "Error",
            });
    }
}
