using Asp.Versioning;
using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.Indexer;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Api.Management.Controllers.Indexer;

/// <summary>
/// API controller responsible for providing detailed information and operations for a specific indexer in the management system.
/// </summary>
[ApiVersion("1.0")]
public class DetailsIndexerController : IndexerControllerBase
{
    private readonly IIndexPresentationFactory _indexPresentationFactory;
    private readonly IExamineManager _examineManager;
    private readonly IMemberIndexAuthorizer _memberIndexAuthorizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DetailsIndexerController"/> class.
    /// </summary>
    /// <param name="indexPresentationFactory">Factory used to create index presentation models.</param>
    /// <param name="examineManager">The <see cref="IExamineManager"/> instance used for managing indexers.</param>
    /// <param name="memberIndexAuthorizer">The <see cref="IMemberIndexAuthorizer"/> used to authorize access to member data.</param>
    [ActivatorUtilitiesConstructor]
    public DetailsIndexerController(
        IIndexPresentationFactory indexPresentationFactory,
        IExamineManager examineManager,
        IMemberIndexAuthorizer memberIndexAuthorizer)
    {
        _indexPresentationFactory = indexPresentationFactory;
        _examineManager = examineManager;
        _memberIndexAuthorizer = memberIndexAuthorizer;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DetailsIndexerController"/> class.
    /// </summary>
    /// <param name="indexPresentationFactory">Factory used to create index presentation models.</param>
    /// <param name="examineManager">The <see cref="IExamineManager"/> instance used for managing indexers.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public DetailsIndexerController(
        IIndexPresentationFactory indexPresentationFactory,
        IExamineManager examineManager)
        : this(
            indexPresentationFactory,
            examineManager,
            StaticServiceProvider.Instance.GetRequiredService<IMemberIndexAuthorizer>())
    {
    }

    /// <summary>
    ///     Check if the index has been rebuilt
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="indexName">The name of the index.</param>
    /// <returns>The index details.</returns>
    /// <remarks>
    ///     This is kind of rudimentary since there's no way we can know that the index has rebuilt, we
    ///     have a listener for the index op complete so we'll just check if that id is no longer there in the runtime cache
    /// </remarks>
    [HttpGet("{indexName}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IndexResponseModel), StatusCodes.Status200OK)]
    [EndpointSummary("Gets indexer details.")]
    [EndpointDescription("Gets detailed information about the indexer identified by the provided name.")]
    public async Task<ActionResult<IndexResponseModel?>> Details(CancellationToken cancellationToken, string indexName)
    {
        if (_examineManager.TryGetIndex(indexName, out IIndex? index))
        {
            // Member index diagnostics expose member property aliases and counts, so they additionally
            // require access to the members section.
            if (_memberIndexAuthorizer.IsMemberIndex(index)
                && await _memberIndexAuthorizer.HasAccessAsync(User) is false)
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            return await _indexPresentationFactory.CreateAsync(index);
        }

        var invalidModelProblem = new ProblemDetails
        {
            Title = "Index Not Found",
            Detail = $"No index found with name = {indexName}",
            Status = StatusCodes.Status400BadRequest,
            Type = "Error",
        };

        return NotFound(invalidModelProblem);
    }
}
