using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.Stylesheet.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.Stylesheet.Folder;

[ApiVersion("1.0")]
public class CreateStylesheetFolderController : StylesheetFolderControllerBase
{
    private readonly IStylesheetFolderService _stylesheetFolderService;
    private readonly IUmbracoMapper _mapper;

    public CreateStylesheetFolderController(IStylesheetFolderService stylesheetFolderService, IUmbracoMapper mapper)
    {
        _stylesheetFolderService = stylesheetFolderService;
        _mapper = mapper;
    }


    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreateStylesheetFolderRequestModel requestModel)
    {
        StylesheetFolderCreateModel createModel = _mapper.Map<StylesheetFolderCreateModel>(requestModel)!;
        Attempt<StylesheetFolderModel?, StylesheetFolderOperationStatus> result = await _stylesheetFolderService.CreateAsync(createModel);

        return result.Success
            ? CreatedAtPath<ByPathStylesheetFolderController>(controller => nameof(controller.ByPath), result.Result!.Path.SystemPathToVirtualPath())
            : OperationStatusResult(result.Status);
    }
}
