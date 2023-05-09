using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Management.Controllers.PublishedCache;

[ApiVersion("1.0")]
public class RebuildPublishedCacheController : PublishedCacheControllerBase
{
    private readonly IPublishedSnapshotService _publishedSnapshotService;

    public RebuildPublishedCacheController(IPublishedSnapshotService publishedSnapshotService)
        => _publishedSnapshotService = publishedSnapshotService;

    [HttpPost("rebuild")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Rebuild()
    {
        _publishedSnapshotService.Rebuild();
        return await Task.FromResult(Ok());
    }
}
