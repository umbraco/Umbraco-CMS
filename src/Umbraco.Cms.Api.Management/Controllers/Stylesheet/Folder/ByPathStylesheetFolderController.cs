using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Folder;

[ApiVersion("1.0")]
public class ByPathStylesheetFolderController : StylesheetFolderControllerBase
{
    public ByPathStylesheetFolderController(IUmbracoMapper mapper, IStylesheetFolderService stylesheetFolderService) : base(mapper, stylesheetFolderService)
    {
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<IActionResult> ByPath(string path) => GetFolderAsync(path);
}
