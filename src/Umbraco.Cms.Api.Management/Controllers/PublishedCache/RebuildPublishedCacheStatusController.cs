using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.PublishedCache;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Management.Controllers.PublishedCache;

/// <summary>
/// Controller for retrieving the status of the published cache rebuild process.
/// </summary>
[ApiVersion("1.0")]
public class RebuildPublishedCacheStatusController : PublishedCacheControllerBase
{
    private readonly IDatabaseCacheRebuilder _databaseCacheRebuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="RebuildPublishedCacheStatusController"/> class,
    /// which manages the status of the published cache rebuild process.
    /// </summary>
    /// <param name="databaseCacheRebuilder">An instance responsible for rebuilding the database cache.</param>
    public RebuildPublishedCacheStatusController(IDatabaseCacheRebuilder databaseCacheRebuilder) => _databaseCacheRebuilder = databaseCacheRebuilder;

    [HttpGet("rebuild/status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(RebuildStatusModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets the rebuild cache status.")]
    [EndpointDescription("Gets the current status of the published content cache rebuild operation.")]
    public async Task<IActionResult> Status(CancellationToken cancellationToken)
    {
        var isRebuilding = await _databaseCacheRebuilder.IsRebuildingAsync();
        return Ok(
            new RebuildStatusModel
            {
                IsRebuilding = isRebuilding,
            });
    }
}
