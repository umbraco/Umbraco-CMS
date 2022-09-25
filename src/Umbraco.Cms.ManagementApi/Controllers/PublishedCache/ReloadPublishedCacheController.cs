using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.PublishedCache;

public class ReloadPublishedCacheController : PublishedCacheControllerBase
{
    private readonly DistributedCache _distributedCache;

    public ReloadPublishedCacheController(DistributedCache distributedCache) => _distributedCache = distributedCache;

    [HttpPost("reload")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Reload()
    {
        _distributedCache.RefreshAllPublishedSnapshot();
        return await Task.FromResult(Ok());
    }
}

