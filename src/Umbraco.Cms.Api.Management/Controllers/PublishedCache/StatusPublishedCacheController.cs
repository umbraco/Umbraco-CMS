using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Management.Controllers.PublishedCache;

[ApiVersion("1.0")]
[Obsolete("This no longer relevant since snapshots are no longer used")]
public class StatusPublishedCacheController : PublishedCacheControllerBase
{
    [HttpGet("status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> Status(CancellationToken cancellationToken)
        => await Task.FromResult(Ok("Obsoleted"));
}
