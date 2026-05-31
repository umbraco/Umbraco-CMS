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

/// <summary>
/// Provides API endpoints for managing stylesheet folders in Umbraco by their path.
/// </summary>
[ApiVersion("1.0")]
public class ByPathStylesheetFolderController : StylesheetFolderControllerBase
{
    private readonly IStylesheetFolderService _stylesheetFolderService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ByPathStylesheetFolderController"/> class, which manages stylesheet folders by their path.
    /// </summary>
    /// <param name="stylesheetFolderService">An instance of <see cref="IStylesheetFolderService"/> used to manage stylesheet folders.</param>
    /// <param name="mapper">An instance of <see cref="IUmbracoMapper"/> used for mapping between models.</param>
    public ByPathStylesheetFolderController(IStylesheetFolderService stylesheetFolderService, IUmbracoMapper mapper)
    {
        _stylesheetFolderService = stylesheetFolderService;
        _mapper = mapper;
    }

    [HttpGet("{*path}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(StylesheetFolderResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Gets a stylesheet folder by path.")]
    [EndpointDescription("Gets a stylesheet folder identified by the provided file path.")]
    public async Task<IActionResult> ByPath(CancellationToken cancellationToken, string path)
    {
        path = DecodePath(path).VirtualPathToSystemPath();
        StylesheetFolderModel? folder = await _stylesheetFolderService.GetAsync(path);
        return folder is not null
            ? Ok(_mapper.Map<StylesheetFolderResponseModel>(folder))
            : OperationStatusResult(StylesheetFolderOperationStatus.NotFound);
    }
}
