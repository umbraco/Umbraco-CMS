using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Preview;

/// <summary>
/// Provides API endpoints for ending content preview sessions in the management interface.
/// </summary>
[ApiVersion("1.0")]
public class EndPreviewController : PreviewControllerBase
{
    private readonly IPreviewService _previewService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndPreviewController"/> class.
    /// </summary>
    /// <param name="previewService">The <see cref="IPreviewService"/> used to manage preview functionality.</param>
    public EndPreviewController(IPreviewService previewService) => _previewService = previewService;

    /// <summary>
    /// Exits preview mode and returns to the normal back office viewing experience.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    [HttpDelete]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Exits preview mode.")]
    [EndpointDescription("Exits preview mode and returns to the normal back office viewing experience.")]
    [AllowAnonymous] // It's okay the client can do this from the website without having a token
    public async Task<IActionResult> End(CancellationToken cancellationToken)
    {
        await _previewService.EndPreviewAsync();
        return Ok();
    }
}
