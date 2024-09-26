using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Management.Controllers.PublishedCache;

[ApiVersion("1.0")]
public class StatusPublishedCacheController : PublishedCacheControllerBase
{
    private readonly IPublishedSnapshotStatus _publishedSnapshotStatus;

    public StatusPublishedCacheController(IPublishedSnapshotStatus publishedSnapshotStatus)
        => _publishedSnapshotStatus = publishedSnapshotStatus;

    [HttpGet("status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public Task<ActionResult<string>> Status(CancellationToken cancellationToken)
        => Task.FromResult<ActionResult<string>>(Ok(_publishedSnapshotStatus.GetStatus()));
}
