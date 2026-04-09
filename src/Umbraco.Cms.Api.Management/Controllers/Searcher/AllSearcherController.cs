using Asp.Versioning;
using Examine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Searcher;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Searcher;

/// <summary>
/// Controller responsible for handling search operations across all searchable entities in the management API.
/// Provides endpoints to perform searches spanning multiple entity types.
/// </summary>
[ApiVersion("1.0")]
public class AllSearcherController : SearcherControllerBase
{
    private readonly IExamineManager _examineManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="AllSearcherController"/> class, providing access to search functionality via the specified <see cref="IExamineManager"/>.
    /// </summary>
    /// <param name="examineManager">The <see cref="IExamineManager"/> instance used to manage and perform search operations.</param>
    public AllSearcherController(IExamineManager examineManager) => _examineManager = examineManager;

    /// <summary>
    ///     Get the details for searchers
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<SearcherResponse>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets a collection of searchers.")]
    [EndpointDescription("Gets a collection of configured searchers in the Umbraco installation.")]
    public Task<ActionResult<PagedViewModel<SearcherResponse>>> All(
        CancellationToken cancellationToken,
        int skip = 0,
        int take = 100)
    {
        var searchers = new List<SearcherResponse>(
            _examineManager.RegisteredSearchers.Select(searcher => new SearcherResponse { Name = searcher.Name })
                .OrderBy(x =>
                    x.Name.TrimEnd("Searcher"))); // order by name , but strip the "Searcher" from the end if it exists
        var viewModel = new PagedViewModel<SearcherResponse>
        {
            Items = searchers.Skip(skip).Take(take),
            Total = searchers.Count,
        };

        return Task.FromResult<ActionResult<PagedViewModel<SearcherResponse>>>(Ok(viewModel));
    }
}
