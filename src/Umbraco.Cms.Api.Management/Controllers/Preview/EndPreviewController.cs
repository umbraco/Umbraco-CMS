using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Preview;

[ApiVersion("1.0")]
public class EndPreviewController : PreviewControllerBase
{
    private readonly IPreviewService _previewService;

    public EndPreviewController(IPreviewService previewService) => _previewService = previewService;

    [HttpDelete]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult End(CancellationToken cancellationToken)
    {
        _previewService.EndPreview();
        return Ok();
    }
}
