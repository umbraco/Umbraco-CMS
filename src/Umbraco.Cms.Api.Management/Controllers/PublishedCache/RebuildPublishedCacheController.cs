using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Management.Controllers.PublishedCache;

/// <summary>
/// API controller responsible for rebuilding the published content cache in Umbraco.
/// </summary>
[ApiVersion("1.0")]
public class RebuildPublishedCacheController : PublishedCacheControllerBase
{
    private readonly IDatabaseCacheRebuilder _databaseCacheRebuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="RebuildPublishedCacheController"/> class with the specified database cache rebuilder.
    /// </summary>
    /// <param name="databaseCacheRebuilder">
    /// An instance of <see cref="IDatabaseCacheRebuilder"/> used to rebuild the published cache.
    /// </param>
    public RebuildPublishedCacheController(IDatabaseCacheRebuilder databaseCacheRebuilder) => _databaseCacheRebuilder = databaseCacheRebuilder;

    /// <summary>
    /// Initiates a rebuild of the entire published content cache.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating the result of the rebuild operation:
    /// returns <c>200 OK</c> if the rebuild is successful, or <c>409 Conflict</c> if a rebuild is already in progress.
    /// </returns>
    [HttpPost("rebuild")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Rebuilds the published content cache.")]
    [EndpointDescription("Rebuilds the entire published content cache from scratch.")]
    public async Task<IActionResult> Rebuild(CancellationToken cancellationToken)
    {
        Attempt<DatabaseCacheRebuildResult> attempt = await _databaseCacheRebuilder.RebuildAsync(true);
        if (attempt is { Success: false, Result: DatabaseCacheRebuildResult.AlreadyRunning })
        {
            var problemDetails = new ProblemDetails
            {
                Title = "Database cache cannot be rebuilt",
                Detail = "The database cache is in the process of rebuilding.",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };
            return Conflict(problemDetails);
        }

        return Ok();
    }
}
