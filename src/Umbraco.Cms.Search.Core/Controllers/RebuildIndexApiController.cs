using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Controllers;

/// <summary>
/// Triggers a full rebuild of a content index, distributed across all servers.
/// </summary>
[ApiVersion("1.0")]
public class RebuildIndexApiController : ApiControllerBase
{
    private readonly IDistributedContentIndexRebuilder _distributedContentIndexRebuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="RebuildIndexApiController"/> class.
    /// </summary>
    /// <param name="distributedContentIndexRebuilder">The service used to trigger the rebuild across all servers.</param>
    public RebuildIndexApiController(IDistributedContentIndexRebuilder distributedContentIndexRebuilder)
        => _distributedContentIndexRebuilder = distributedContentIndexRebuilder;

    /// <summary>
    /// Rebuilds the specified content index.
    /// </summary>
    /// <param name="indexAlias">The alias of the index to rebuild.</param>
    /// <returns>200 OK if the rebuild was triggered; otherwise an error.</returns>
    [HttpPut("rebuild")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Rebuild(string indexAlias)
    {
        if (string.IsNullOrWhiteSpace(indexAlias))
        {
            return BadRequest("The indexAlias parameter must be provided and cannot be empty.");
        }

        return _distributedContentIndexRebuilder.Rebuild(indexAlias)
            ? Ok()
            : BadRequest("Could not rebuild the index with the specified index alias. See the log for details.");
    }
}
