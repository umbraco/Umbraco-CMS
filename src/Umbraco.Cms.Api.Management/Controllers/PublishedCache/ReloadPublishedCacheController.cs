using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.PublishedCache;

[ApiVersion("1.0")]
public class ReloadPublishedCacheController : PublishedCacheControllerBase
{
    private readonly DistributedCache _distributedCache;

    public ReloadPublishedCacheController(DistributedCache distributedCache) => _distributedCache = distributedCache;

    [HttpPost("reload")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<IActionResult> Reload(CancellationToken cancellationToken)
    {
        _distributedCache.RefreshAllPublishedSnapshot();
        return Task.FromResult<IActionResult>(Ok());
    }
}

