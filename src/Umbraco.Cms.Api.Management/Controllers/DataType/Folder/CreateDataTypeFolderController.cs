using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Folder;

[ApiVersion("1.0")]
public class CreateDataTypeFolderController : DataTypeFolderControllerBase
{
    public CreateDataTypeFolderController(
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IDataTypeContainerService dataTypeContainerService)
        : base(backOfficeSecurityAccessor, dataTypeContainerService)
    {
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Creates a data type folder.")]
    [EndpointDescription("Creates a data type folder with the details provided in the request model.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken, CreateFolderRequestModel createFolderRequestModel)
        => await CreateFolderAsync<ByKeyDataTypeFolderController>(
            createFolderRequestModel,
            controller => nameof(controller.ByKey));
}
