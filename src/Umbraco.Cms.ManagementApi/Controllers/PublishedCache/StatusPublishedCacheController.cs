using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.ManagementApi.Controllers.PublishedCache;

public class StatusPublishedCacheController : PublishedCacheControllerBase
{
    private readonly IPublishedSnapshotStatus _publishedSnapshotStatus;

    public StatusPublishedCacheController(IPublishedSnapshotStatus publishedSnapshotStatus)
        => _publishedSnapshotStatus = publishedSnapshotStatus;

    [HttpGet("status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> Status()
        => await Task.FromResult(Ok(_publishedSnapshotStatus.GetStatus()));
}
