using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Search;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Search.Configuration;
using Umbraco.Cms.Core.Search.Indexing;

namespace Umbraco.Cms.Api.Management.Controllers.Search.Index;

/// <summary>
/// Lists all registered content indexes with their provider name, document count, and health status.
/// </summary>
[ApiVersion("1.0")]
public class GetAllIndexesApiController : SearchControllerBase
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
    [ProducesResponseType<PagedViewModel<IndexResponseModel>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Indexes()
    {
        List<IndexResponseModel> indexes = [];
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
                new IndexResponseModel
                {
                    IndexAlias = indexRegistration.IndexAlias,
                    ProviderName = indexMetadata.ProviderName,
                    DocumentCount = indexMetadata.DocumentCount,
                    HealthStatus = indexMetadata.HealthStatus,
                });
        }

        return Ok(new PagedViewModel<IndexResponseModel> { Items = indexes, Total = indexes.Count });
    }
}
