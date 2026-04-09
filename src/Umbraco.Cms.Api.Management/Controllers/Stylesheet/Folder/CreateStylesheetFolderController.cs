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

/// <summary>
/// Controller for creating stylesheet folders via the Umbraco CMS Management API.
/// </summary>
[ApiVersion("1.0")]
public class CreateStylesheetFolderController : StylesheetFolderControllerBase
{
    private readonly IStylesheetFolderService _stylesheetFolderService;
    private readonly IUmbracoMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateStylesheetFolderController"/> class, responsible for handling stylesheet folder creation requests.
    /// </summary>
    /// <param name="stylesheetFolderService">Service used to manage stylesheet folders.</param>
    /// <param name="mapper">The Umbraco object mapper for mapping between models.</param>
    public CreateStylesheetFolderController(IStylesheetFolderService stylesheetFolderService, IUmbracoMapper mapper)
    {
        _stylesheetFolderService = stylesheetFolderService;
        _mapper = mapper;
    }


    /// <summary>
    /// Creates a new stylesheet folder with the specified name and parent location.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="requestModel">The model containing the details of the stylesheet folder to create.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> that represents the result of the operation:
    /// returns <c>201 Created</c> with the location of the new folder if successful;
    /// <c>400 Bad Request</c> if the request is invalid;
    /// or <c>404 Not Found</c> if the parent location does not exist.
    /// </returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a stylesheet folder.")]
    [EndpointDescription("Creates a new stylesheet folder with the provided name and parent location.")]
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
