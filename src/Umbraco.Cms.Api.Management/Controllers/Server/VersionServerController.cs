using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Api.Management.ViewModels.Server;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Controllers.Server;

[ApiVersion("1.0")]
public class VersionServerController : ServerControllerBase
{
    private readonly IUmbracoVersion _umbracoVersion;

    public VersionServerController(IUmbracoVersion umbracoVersion) => _umbracoVersion = umbracoVersion;

    [HttpGet("version")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(VersionResponseModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<VersionResponseModel>> Get() =>
        await Task.FromResult(new VersionResponseModel
        {
            Version = _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild()
        });
}
