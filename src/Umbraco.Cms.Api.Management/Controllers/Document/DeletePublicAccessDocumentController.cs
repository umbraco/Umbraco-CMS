using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Document;

public class DeletePublicAccessDocumentController : DocumentControllerBase
{
    private readonly IPublicAccessService _publicAccessService;

    public DeletePublicAccessDocumentController(IPublicAccessService publicAccessService) => _publicAccessService = publicAccessService;

    [MapToApiVersion("1.0")]
    [HttpDelete("{id:guid}/public-access")]
    public async Task<IActionResult> CreatePublicAccess(Guid id)
    {
        await _publicAccessService.DeleteAsync(id);

        return Ok();
    }
}
