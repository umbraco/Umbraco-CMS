using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Preview;

[ApiVersion("1.0")]
public class EnterPreviewController : PreviewControllerBase
{
    private readonly IPreviewService _previewService;

    public EnterPreviewController(IPreviewService previewService) => _previewService = previewService;

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Enter(CancellationToken cancellationToken)
    {
        _previewService.EnterPreview();
        return Ok();
    }
}
