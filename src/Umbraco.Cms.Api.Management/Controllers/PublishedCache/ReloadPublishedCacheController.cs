using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.PublishedCache;

/// <summary>
/// API controller responsible for handling requests to reload the published cache in Umbraco.
/// </summary>
[ApiVersion("1.0")]
public class ReloadPublishedCacheController : PublishedCacheControllerBase
{
    private readonly DistributedCache _distributedCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReloadPublishedCacheController"/> class.
    /// </summary>
    /// <param name="distributedCache">The <see cref="DistributedCache"/> instance used for cache synchronization.</param>
    public ReloadPublishedCacheController(DistributedCache distributedCache) => _distributedCache = distributedCache;

    [HttpPost("reload")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Reloads the published content cache.")]
    [EndpointDescription("Reloads the published content cache from the database.")]
    public Task<IActionResult> Reload(CancellationToken cancellationToken)
    {
        _distributedCache.RefreshAllPublishedSnapshot();
        return Task.FromResult<IActionResult>(Ok());
    }
}
