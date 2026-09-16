using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Search.Core.Configuration;
using Umbraco.Cms.Search.Core.Models.Configuration;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Models.ViewModels;
using Umbraco.Cms.Search.Core.Services;

namespace Umbraco.Cms.Search.Core.Controllers;

/// <summary>
/// Lists all registered content indexes with their provider name, document count, and health status.
/// </summary>
[ApiVersion("1.0")]
public class GetAllIndexesApiController : ApiControllerBase
{
    private readonly IIndexerResolver _indexerResolver;
    private readonly IndexOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllIndexesApiController"/> class.
    /// </summary>
    /// <param name="indexerResolver">The resolver used to obtain the indexer for each registered index alias.</param>
    /// <param name="options">The registered content index configuration.</param>
    public GetAllIndexesApiController(IIndexerResolver indexerResolver, IOptions<IndexOptions> options)
    {
        _indexerResolver = indexerResolver;
        _options = options.Value;
    }

    /// <summary>
    /// Gets all registered content indexes.
    /// </summary>
    /// <returns>A paged model of index view models.</returns>
    [HttpGet("indexes")]
    [ProducesResponseType<PagedViewModel<IndexViewModel>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Indexes()
    {
        List<IndexViewModel> indexes = [];
        foreach (ContentIndexRegistration indexRegistration in _options.GetContentIndexRegistrations())
        {
            IIndexer? indexer = _indexerResolver.GetIndexer(indexRegistration.IndexAlias);
            if (indexer is null)
            {
                // NOTE: logging is handled by the resolver
                continue;
            }

            IndexMetadata indexMetadata = await indexer.GetMetadataAsync(indexRegistration.IndexAlias);

            indexes.Add(
                new IndexViewModel
                {
                    IndexAlias = indexRegistration.IndexAlias,
                    ProviderName = indexMetadata.ProviderName,
                    DocumentCount = indexMetadata.DocumentCount,
                    HealthStatus = indexMetadata.HealthStatus,
                });
        }

        return Ok(new PagedViewModel<IndexViewModel> { Items = indexes, Total = indexes.Count });
    }
}
