using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Package;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Packaging;

namespace Umbraco.Cms.Api.Management.Controllers.Package;

public class EmptyPackageController : PackageControllerBase
{
    private readonly IUmbracoMapper _umbracoMapper;

    public EmptyPackageController(IUmbracoMapper umbracoMapper)
    {
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets an empty package to use as a scaffold when creating a new package.
    /// </summary>
    /// <returns>The empty package.</returns>
    [HttpGet("empty")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PackageDefinitionViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<PackageDefinitionViewModel>> Empty()
    {
        return await Task.FromResult(Ok(_umbracoMapper.Map<PackageDefinitionViewModel>(new PackageDefinition())));
    }
}
