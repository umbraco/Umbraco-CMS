using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Folder;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Folder;

[ApiVersion("1.0")]
public class CreatePartialViewFolderController : PartialViewFolderControllerBase
{
    private readonly IPartialViewFolderService _partialViewFolderService;
    private readonly IUmbracoMapper _mapper;

    public CreatePartialViewFolderController(IPartialViewFolderService partialViewFolderService, IUmbracoMapper mapper)
    {
        _partialViewFolderService = partialViewFolderService;
        _mapper = mapper;
    }


    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(
        CancellationToken cancellationToken,
        CreatePartialViewFolderRequestModel requestModel)
    {
        PartialViewFolderCreateModel createModel = _mapper.Map<PartialViewFolderCreateModel>(requestModel)!;
        Attempt<PartialViewFolderModel?, PartialViewFolderOperationStatus> result = await _partialViewFolderService.CreateAsync(createModel);

        return result.Success
            ? CreatedAtPath<ByPathPartialViewFolderController>(controller => nameof(controller.ByPath), result.Result!.Path.SystemPathToVirtualPath())
            : OperationStatusResult(result.Status);
    }
}
