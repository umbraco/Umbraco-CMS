using Asp.Versioning;
using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Indexer;

/// <summary>
/// API controller responsible for handling requests to rebuild the search indexer in the Umbraco CMS management context.
/// </summary>
[ApiVersion("1.0")]
public class RebuildIndexerController : IndexerControllerBase
{
    private readonly ILogger<RebuildIndexerController> _logger;
    private readonly IIndexingRebuilderService _indexingRebuilderService;
    private readonly IExamineManager _examineManager;
    private readonly IMemberIndexAuthorizer _memberIndexAuthorizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RebuildIndexerController"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{RebuildIndexerController}"/> used for logging.</param>
    /// <param name="indexingRebuilderService">The <see cref="IIndexingRebuilderService"/> responsible for rebuilding search indexes.</param>
    /// <param name="examineManager">The <see cref="IExamineManager"/> instance for managing Examine indexes.</param>
    /// <param name="memberIndexAuthorizer">The <see cref="IMemberIndexAuthorizer"/> used to authorize access to member data.</param>
    [ActivatorUtilitiesConstructor]
    public RebuildIndexerController(
        ILogger<RebuildIndexerController> logger,
        IIndexingRebuilderService indexingRebuilderService,
        IExamineManager examineManager,
        IMemberIndexAuthorizer memberIndexAuthorizer)
    {
        _logger = logger;
        _indexingRebuilderService = indexingRebuilderService;
        _examineManager = examineManager;
        _memberIndexAuthorizer = memberIndexAuthorizer;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RebuildIndexerController"/> class.
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{RebuildIndexerController}"/> used for logging.</param>
    /// <param name="indexingRebuilderService">The <see cref="IIndexingRebuilderService"/> responsible for rebuilding search indexes.</param>
    /// <param name="examineManager">The <see cref="IExamineManager"/> instance for managing Examine indexes.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public RebuildIndexerController(
        ILogger<RebuildIndexerController> logger,
        IIndexingRebuilderService indexingRebuilderService,
        IExamineManager examineManager)
        : this(
            logger,
            indexingRebuilderService,
            examineManager,
            StaticServiceProvider.Instance.GetRequiredService<IMemberIndexAuthorizer>())
    {
    }

    /// <summary>
    ///     Rebuilds the index.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="indexName">The name of the index to rebuild.</param>
    /// <returns>The result of the rebuild operation.</returns>
    [HttpPost("{indexName}/rebuild")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("Rebuilds an indexer.")]
    [EndpointDescription("Rebuilds the search index for the indexer identified by the provided name.")]
    public async Task<IActionResult> Rebuild(CancellationToken cancellationToken, string indexName)
    {
        if (!_examineManager.TryGetIndex(indexName, out IIndex? index))
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Index Not Found",
                Detail = $"No index found with name = {indexName}",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };

            return NotFound(invalidModelProblem);
        }

        // Rebuilding a member index acts on member data, so it additionally requires access to the
        // members section - consistent with the index being hidden from users without that access.
        if (_memberIndexAuthorizer.IsMemberIndex(index)
            && await _memberIndexAuthorizer.HasAccessAsync(User) is false)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        if (!_indexingRebuilderService.CanRebuild(index.Name))
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Could not validate the populator",
                Detail =
                    $"The index {index.Name} could not be rebuilt because we could not validate its associated {typeof(IIndexPopulator)}",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };

            return BadRequest(invalidModelProblem);
        }

        _logger.LogInformation("Rebuilding index '{IndexName}'", indexName);

        if (await _indexingRebuilderService.TryRebuildAsync(index, indexName))
        {
            return Ok();
        }

        var problemDetails = new ProblemDetails
        {
            Title = "Index could not be rebuilt",
            Detail = $"The index {index.Name} could not be rebuild. Check the log for details on this error.",
            Status = StatusCodes.Status400BadRequest,
            Type = "Error",
        };

        return Conflict(problemDetails);
    }
}
