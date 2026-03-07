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

    /// <summary>
    /// Provides API endpoints for managing partial view folders in Umbraco by their specified path.
    /// </summary>
[ApiVersion("1.0")]
public class ByPathPartialViewFolderController : PartialViewFolderControllerBase
{
    private readonly IPartialViewFolderService _partialViewFolderService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByPathPartialViewFolderController"/> class.
    /// </summary>
    /// <param name="partialViewFolderService">Service used to manage partial view folders.</param>
    /// <param name="mapper">The mapper used for Umbraco object mapping.</param>
    public ByPathPartialViewFolderController(IPartialViewFolderService partialViewFolderService, IUmbracoMapper mapper)
    {
        _partialViewFolderService = partialViewFolderService;
        _mapper = mapper;
    }

    /// <summary>
    /// Retrieves a partial view folder by its file path.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="path">The file path of the partial view folder to retrieve.</param>
    /// <returns>An <see cref="IActionResult"/> containing the partial view folder response model if found; otherwise, a not found result.</returns>
    [HttpGet("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PartialViewFolderResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a partial view folder by path.")]
    [EndpointDescription("Gets a partial view folder identified by the provided file path.")]
    public async Task<IActionResult> ByPath(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        PartialViewFolderModel? folder = await _partialViewFolderService.GetAsync(path);
        return folder is not null
            ? Ok(_mapper.Map<PartialViewFolderResponseModel>(folder))
            : OperationStatusResult(PartialViewFolderOperationStatus.NotFound);
    }
}
