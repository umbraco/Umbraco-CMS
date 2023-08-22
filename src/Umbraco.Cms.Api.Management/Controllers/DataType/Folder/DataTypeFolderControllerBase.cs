using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Builders;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Folder;

[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.DataType}/folder")]
[ApiExplorerSettings(GroupName = "Data Type")]
[Authorize(Policy = "New" + AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes)]
public abstract class DataTypeFolderControllerBase : FolderManagementControllerBase<DataTypeContainerOperationStatus>
{
    private readonly IDataTypeContainerService _dataTypeContainerService;

    protected DataTypeFolderControllerBase(IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IDataTypeContainerService dataTypeContainerService)
        : base(backOfficeSecurityAccessor) =>
        _dataTypeContainerService = dataTypeContainerService;

    protected override Guid ContainerObjectType => Constants.ObjectTypes.DataType;

    protected override async Task<EntityContainer?> GetContainerAsync(Guid id)
        => await _dataTypeContainerService.GetAsync(id);

    protected override async Task<EntityContainer?> GetParentContainerAsync(EntityContainer container)
        => await _dataTypeContainerService.GetParentAsync(container);

    protected override async Task<Attempt<EntityContainer, DataTypeContainerOperationStatus>> CreateContainerAsync(EntityContainer container, Guid? parentId, Guid userKey)
        => await _dataTypeContainerService.CreateAsync(container, parentId, userKey);

    protected override async Task<Attempt<EntityContainer, DataTypeContainerOperationStatus>> UpdateContainerAsync(EntityContainer container, Guid userKey)
        => await _dataTypeContainerService.UpdateAsync(container, userKey);

    protected override async Task<Attempt<EntityContainer?, DataTypeContainerOperationStatus>> DeleteContainerAsync(Guid id, Guid userKey)
        => await _dataTypeContainerService.DeleteAsync(id, userKey);

    protected override IActionResult OperationStatusResult(DataTypeContainerOperationStatus status)
        => status switch
        {
            DataTypeContainerOperationStatus.NotFound => NotFound(new ProblemDetailsBuilder()
                    .WithTitle("The data type folder could not be found")
                    .Build()),
            DataTypeContainerOperationStatus.ParentNotFound => NotFound(new ProblemDetailsBuilder()
                    .WithTitle("The data type parent folder could not be found")
                    .Build()),
            DataTypeContainerOperationStatus.DuplicateName => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("The name is already used")
                .WithDetail("The data type folder name must be unique on this parent.")
                .Build()),
            DataTypeContainerOperationStatus.DuplicateKey => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("The id is already used")
                .WithDetail("The data type folder id must be unique.")
                .Build()),
            DataTypeContainerOperationStatus.NotEmpty => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("The folder is not empty")
                .WithDetail("The data type folder must be empty to perform this action.")
                .Build()),
            DataTypeContainerOperationStatus.CancelledByNotification => BadRequest(new ProblemDetailsBuilder()
                .WithTitle("Cancelled by notification")
                .WithDetail("A notification handler prevented the data type folder operation.")
                .Build()),
            _ => StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetailsBuilder()
                .WithTitle("Unknown data type folder operation status.")
                .Build()),
        };
}
