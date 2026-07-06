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

[ApiVersion("1.0")]
public class GetAllIndexesApiController : ApiControllerBase
{
    private readonly IIndexerResolver _indexerResolver;
    private readonly IndexOptions _options;

    public GetAllIndexesApiController(IIndexerResolver indexerResolver, IOptions<IndexOptions> options)
    {
        _indexerResolver = indexerResolver;
        _options = options.Value;
    }

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
