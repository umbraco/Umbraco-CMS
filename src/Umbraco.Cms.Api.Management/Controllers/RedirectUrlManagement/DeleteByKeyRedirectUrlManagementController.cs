using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

[ApiVersion("1.0")]
public class DeleteByKeyRedirectUrlManagementController : RedirectUrlManagementControllerBase
{
    private readonly IRedirectUrlService _redirectUrlService;

    public DeleteByKeyRedirectUrlManagementController(IRedirectUrlService redirectUrlService)
    {
        _redirectUrlService = redirectUrlService;
    }

    [MapToApiVersion("1.0")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<IActionResult> DeleteByKey(CancellationToken cancellationToken, Guid id)
    {
        _redirectUrlService.Delete(id);
        return Task.FromResult<IActionResult>(Ok());
    }
}
