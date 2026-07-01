using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.TemporaryFile;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.TemporaryFile;

/// <summary>
/// API controller responsible for handling the deletion of temporary files in the Umbraco CMS management context.
/// </summary>
[ApiVersion("1.0")]
public class DeleteTemporaryFileController : TemporaryFileControllerBase
{
    private readonly ITemporaryFileService _temporaryFileService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTemporaryFileController"/> class with the specified temporary file service.
    /// </summary>
    /// <param name="temporaryFileService">An instance of <see cref="ITemporaryFileService"/> used to manage temporary files.</param>
    public DeleteTemporaryFileController(ITemporaryFileService temporaryFileService) => _temporaryFileService = temporaryFileService;

    /// <summary>Deletes a temporary file identified by the provided Id.</summary>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <param name="id">The unique identifier of the temporary file to delete.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the delete operation.</returns>
    [HttpDelete($"{{{nameof(id)}}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Deletes a temporary file.")]
    [EndpointDescription("Deletes a temporary file identified by the provided Id.")]
    public async Task<IActionResult> Delete(CancellationToken cancellationToken, Guid id)
    {
        Attempt<TemporaryFileModel?, TemporaryFileOperationStatus> result = await _temporaryFileService.DeleteAsync(id);

        return result.Success
            ? Ok()
            : TemporaryFileStatusResult(result.Status);
    }
}
