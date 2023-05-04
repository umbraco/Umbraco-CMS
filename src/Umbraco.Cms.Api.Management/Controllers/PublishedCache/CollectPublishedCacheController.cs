﻿using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Management.Controllers.PublishedCache;

[ApiVersion("1.0")]
public class CollectPublishedCacheController : PublishedCacheControllerBase
{
    private readonly IPublishedSnapshotService _publishedSnapshotService;

    public CollectPublishedCacheController(IPublishedSnapshotService publishedSnapshotService)
        => _publishedSnapshotService = publishedSnapshotService;

    [HttpPost("collect")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Collect()
    {
        GC.Collect();
        await _publishedSnapshotService.CollectAsync();
        return Ok();
    }
}
