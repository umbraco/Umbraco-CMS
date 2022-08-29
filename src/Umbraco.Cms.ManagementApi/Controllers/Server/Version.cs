using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.ManagementApi.ViewModels.Server;
using Umbraco.Extensions;

namespace Umbraco.Cms.ManagementApi.Controllers.Server;

public class Version : ServerController
{
    private readonly IUmbracoVersion _umbracoVersion;

    public Version(IUmbracoVersion umbracoVersion) => _umbracoVersion = umbracoVersion;

    [HttpGet("version")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(VersionViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<VersionViewModel>> Get() =>
        new VersionViewModel { Version = _umbracoVersion.SemanticVersion.ToSemanticStringWithoutBuild() };
}
