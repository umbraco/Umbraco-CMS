using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Api.Management.Controllers.PublishedCache;

[Obsolete("This controller no longer serves a purpose")]
[ApiVersion("1.0")]
public class CollectPublishedCacheController : PublishedCacheControllerBase
{
    [HttpPost("collect")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Collect(CancellationToken cancellationToken)
    {
        return StatusCode(501);
    }
}
