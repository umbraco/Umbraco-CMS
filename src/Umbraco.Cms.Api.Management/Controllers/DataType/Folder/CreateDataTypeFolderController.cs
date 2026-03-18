using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Folder;

/// <summary>
/// Provides API endpoints for creating folders used to organize data types in the system.
/// </summary>
[ApiVersion("1.0")]
public class CreateDataTypeFolderController : DataTypeFolderControllerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateDataTypeFolderController"/> class, responsible for handling requests related to creating data type folders.
    /// </summary>
    /// <param name="backOfficeSecurityAccessor">Provides access to back office security features for authorization and authentication.</param>
    /// <param name="dataTypeContainerService">Service used to manage data type containers (folders) within the system.</param>
    public CreateDataTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IDataTypeContainerService dataTypeContainerService)
        : base(backOfficeSecurityAccessor, dataTypeContainerService)
    {
    }

    /// <summary>
    /// Creates a new data type folder using the specified details.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="createFolderRequestModel">The request model containing the folder name and parent location.</param>
    /// <returns>A <see cref="Task{IActionResult}"/> representing the asynchronous operation result.</returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a data type folder.")]
    [EndpointDescription("Creates a new data type folder with the provided name and parent location.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken, CreateFolderRequestModel createFolderRequestModel)
        => await CreateFolderAsync<ByKeyDataTypeFolderController>(
            createFolderRequestModel,
            controller => nameof(controller.ByKey));
}
