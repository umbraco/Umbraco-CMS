using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

/// <summary>
/// Controller responsible for deleting redirect URLs by their unique key.
/// Handles API requests related to the removal of specific redirect URLs.
/// </summary>
[ApiVersion("1.0")]
public class DeleteByKeyRedirectUrlManagementController : RedirectUrlManagementControllerBase
{
    private readonly IRedirectUrlService _redirectUrlService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteByKeyRedirectUrlManagementController"/> class with the specified redirect URL service.
    /// </summary>
    /// <param name="redirectUrlService">An instance of <see cref="IRedirectUrlService"/> used to manage redirect URLs.</param>
    public DeleteByKeyRedirectUrlManagementController(IRedirectUrlService redirectUrlService)
    {
        _redirectUrlService = redirectUrlService;
    }

    /// <summary>
    /// Deletes the redirect URL with the specified unique identifier.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="id">The unique identifier (GUID) of the redirect URL to delete.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    [MapToApiVersion("1.0")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Deletes a redirect URL.")]
    [EndpointDescription("Deletes a redirect URL identified by the provided Id.")]
    public async Task<IActionResult> DeleteByKey(CancellationToken cancellationToken, Guid id)
    {
        RedirectUrlOperationStatus status = await _redirectUrlService.DeleteWithStatusAsync(id);
        return RedirectUrlOperationStatusResult(status);
    }
}
