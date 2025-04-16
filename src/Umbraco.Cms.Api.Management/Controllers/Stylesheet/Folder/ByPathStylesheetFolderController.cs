using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet.Folder;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Folder;

[ApiVersion("1.0")]
public class ByPathStylesheetFolderController : StylesheetFolderControllerBase
{
    private readonly IStylesheetFolderService _stylesheetFolderService;
    private readonly IUmbracoMapper _mapper;

    public ByPathStylesheetFolderController(IStylesheetFolderService stylesheetFolderService, IUmbracoMapper mapper)
    {
        _stylesheetFolderService = stylesheetFolderService;
        _mapper = mapper;
    }

    [HttpGet("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(StylesheetFolderResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByPath(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        StylesheetFolderModel? folder = await _stylesheetFolderService.GetAsync(path);
        return folder is not null
            ? Ok(_mapper.Map<StylesheetFolderResponseModel>(folder))
            : OperationStatusResult(StylesheetFolderOperationStatus.NotFound);
    }
}
