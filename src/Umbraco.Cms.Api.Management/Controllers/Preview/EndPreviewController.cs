using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Api.Management.Controllers.Preview;

public class EndPreviewController : PreviewControllerBase
{
    private readonly IPreviewService _previewService;

    public EndPreviewController(IPreviewService previewService) => _previewService = previewService;

    [HttpDelete]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Enter()
    {
        _previewService.ExitPreview();
        return Ok();
    }
}
