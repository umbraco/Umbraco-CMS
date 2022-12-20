using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.RedirectUrlManagement;

public class DeleteByKeyRedirectUrlManagementController : RedirectUrlManagementBaseController
{
    private readonly IRedirectUrlService _redirectUrlService;

    public DeleteByKeyRedirectUrlManagementController(IRedirectUrlService redirectUrlService)
    {
        _redirectUrlService = redirectUrlService;
    }

    [HttpDelete("{key:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteByKey(Guid key)
    {
        _redirectUrlService.Delete(key);
        return Ok();
    }
}
