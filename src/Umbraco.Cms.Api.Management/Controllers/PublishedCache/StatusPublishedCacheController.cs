using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Umbraco.Cms.Api.Management.Controllers.PublishedCache;

[ApiVersion("1.0")]
[Obsolete("This no longer relevant since snapshots are no longer used")]
public class StatusPublishedCacheController : PublishedCacheControllerBase
{
    [HttpGet("status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<ActionResult<string>> Status(CancellationToken cancellationToken)
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}
