using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Preview;

/// <summary>
/// Controller responsible for handling requests to enter preview mode in the management API.
/// </summary>
[ApiVersion("1.0")]
[Obsolete("Do not use this. Preview state is initiated implicitly by the preview URL generation. Scheduled for removal in Umbraco 18.")]
public class EnterPreviewController : PreviewControllerBase
{
    private readonly IPreviewService _previewService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnterPreviewController"/> class, which handles preview entry operations in the management API.
    /// </summary>
    /// <param name="previewService">Service used to manage preview functionality.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context and authentication.</param>
    public EnterPreviewController(IPreviewService previewService, IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
    {
        _previewService = previewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
    }

    /// <summary>
    /// Attempts to enter preview mode for the current user session, enabling the viewing of unpublished content.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the operation:
    /// <list type="bullet">
    /// <item><description><c>200 OK</c> if preview mode was successfully entered.</description></item>
    /// <item><description><c>500 Internal Server Error</c> if entering preview mode failed.</description></item>
    /// </list>
    /// </returns>
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
