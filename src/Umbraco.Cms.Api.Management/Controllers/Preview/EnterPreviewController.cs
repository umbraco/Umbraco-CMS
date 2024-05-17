using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Preview;

[ApiVersion("1.0")]
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
    public async Task<IActionResult> Enter(CancellationToken cancellationToken)
    {
        await _previewService.EnterPreviewAsync(CurrentUser(_backOfficeSecurityAccessor));
        return Ok();
    }
}
