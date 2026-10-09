using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Search;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Search.Indexing;

namespace Umbraco.Cms.Api.Management.Controllers.Search.Index;

/// <summary>
/// Gets a single registered index's provider name, document count, and health status.
/// </summary>
[ApiVersion("1.0")]
public class GetIndexApiController : SearchControllerBase
{
    private readonly IIndexerResolver _indexerResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetIndexApiController"/> class.
    /// </summary>
    /// <param name="indexerResolver">The resolver used to obtain the indexer for the requested index alias.</param>
    public GetIndexApiController(IIndexerResolver indexerResolver)
        => _indexerResolver = indexerResolver;

    /// <summary>
    /// Gets the specified index.
    /// </summary>
    /// <param name="indexAlias">The alias of the index.</param>
    /// <returns>The index view model, or an error if the index alias is missing or could not be resolved.</returns>
    [HttpGet("indexes/{indexAlias}")]
    [ProducesResponseType<IndexResponseModel>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Index(string indexAlias)
    {
        if (string.IsNullOrWhiteSpace(indexAlias))
        {
            return BadRequest("The indexAlias parameter must be provided and cannot be empty.");
        }

        IIndexer? indexer = _indexerResolver.GetIndexer(indexAlias);
        if (indexer is null)
        {
            return NotFound("Could not resolve the indexer for the specified index.");
        }

        IndexMetadata indexMetadata = await indexer.GetMetadataAsync(indexAlias);

        return Ok(new IndexResponseModel
        {
            IndexAlias = indexAlias,
            ProviderName = indexMetadata.ProviderName,
            DocumentCount = indexMetadata.DocumentCount,
            HealthStatus = indexMetadata.HealthStatus,
        });
    }
}
