using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Search.Core.Controllers;

[ApiVersion("1.0")]
public class RebuildIndexApiController : ApiControllerBase
{
    private readonly IDistributedContentIndexRebuilder _distributedContentIndexRebuilder;

    public RebuildIndexApiController(IDistributedContentIndexRebuilder distributedContentIndexRebuilder)
        => _distributedContentIndexRebuilder = distributedContentIndexRebuilder;

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
