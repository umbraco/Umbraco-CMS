using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public class DeletePublicAccessDocumentController : DocumentControllerBase
{
    private readonly IPublicAccessService _publicAccessService;

    public DeletePublicAccessDocumentController(IPublicAccessService publicAccessService) => _publicAccessService = publicAccessService;

    [MapToApiVersion("1.0")]
    [HttpDelete("{id:guid}/public-access")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _publicAccessService.DeleteAsync(id);

        return Ok();
    }
}
