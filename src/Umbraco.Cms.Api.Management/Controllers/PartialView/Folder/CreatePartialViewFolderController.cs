using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Folder;

[ApiVersion("1.0")]
public class CreatePartialViewFolderController : PartialViewFolderControllerBase
{
    public CreatePartialViewFolderController(IUmbracoMapper mapper, IPartialViewFolderService partialViewFolderService)
        : base(mapper, partialViewFolderService)
    {
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(CreatePathFolderRequestModel model)
    {
        Attempt<PathContainer?, PartialViewFolderOperationStatus> result = await CreateAsync(model);
        return result.Success
            ? CreatedAtAction<ByPathPartialViewFolderController>(controller => nameof(controller.ByPath), new { path = result.Result!.Path })
            : OperationStatusResult(result.Status);
    }
}
