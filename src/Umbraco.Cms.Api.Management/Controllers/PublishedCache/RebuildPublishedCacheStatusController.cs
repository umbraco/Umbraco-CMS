using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.PublishedCache;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Api.Management.Controllers.PublishedCache;

[ApiVersion("1.0")]
public class RebuildPublishedCacheStatusController : PublishedCacheControllerBase
{
    private readonly IDatabaseCacheRebuilder _databaseCacheRebuilder;

    public RebuildPublishedCacheStatusController(IDatabaseCacheRebuilder databaseCacheRebuilder) => _databaseCacheRebuilder = databaseCacheRebuilder;

    [HttpGet("rebuild/status")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(RebuildStatusModel), StatusCodes.Status200OK)]
    public Task<IActionResult> Status(CancellationToken cancellationToken)
    {
        var isRebuilding = _databaseCacheRebuilder.IsRebuilding();
        return Task.FromResult((IActionResult)Ok(new RebuildStatusModel
        {
            IsRebuilding = isRebuilding
        }));
    }
}
