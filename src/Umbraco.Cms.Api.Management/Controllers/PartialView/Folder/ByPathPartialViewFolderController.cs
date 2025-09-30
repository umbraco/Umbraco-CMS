using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Extensions;
using Umbraco.Cms.Api.Management.ViewModels.PartialView.Folder;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Controllers.PartialView.Folder;

[ApiVersion("1.0")]
public class ByPathPartialViewFolderController : PartialViewFolderControllerBase
{
    private readonly IPartialViewFolderService _partialViewFolderService;
    private readonly IUmbracoMapper _mapper;

    public ByPathPartialViewFolderController(IPartialViewFolderService partialViewFolderService, IUmbracoMapper mapper)
    {
        _partialViewFolderService = partialViewFolderService;
        _mapper = mapper;
    }

    [HttpGet("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PartialViewFolderResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByPath(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        PartialViewFolderModel? folder = await _partialViewFolderService.GetAsync(path);
        return folder is not null
            ? Ok(_mapper.Map<PartialViewFolderResponseModel>(folder))
            : OperationStatusResult(PartialViewFolderOperationStatus.NotFound);
    }
}
