using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Folder;

[ApiVersion("1.0")]
public class CreateStylesheetFolderController : StylesheetFolderControllerBase
{
    public CreateStylesheetFolderController(IUmbracoMapper mapper, IStylesheetFolderService stylesheetFolderService) : base(mapper, stylesheetFolderService)
    {
    }


    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public Task<IActionResult> Create(CreatePathFolderRequestModel model) => CreateAsync(model);
}
