using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Folder;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.UdiEntityType.DataType}/folder")]
[ApiExplorerSettings(GroupName = "Data Type Folder")]
public abstract class DataTypeFolderControllerBase : FolderManagementControllerBase
{
    private readonly IDataTypeService _dataTypeService;

    protected DataTypeFolderControllerBase(IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IDataTypeService dataTypeService)
        : base(backOfficeSecurityAccessor) =>
        _dataTypeService = dataTypeService;

    protected override EntityContainer? GetContainer(Guid key)
        => _dataTypeService.GetContainer(key);

    protected override EntityContainer? GetContainer(int containerId)
        => _dataTypeService.GetContainer(containerId);

    protected override Attempt<OperationResult<OperationResultType, EntityContainer>?> CreateContainer(int parentId, string name, int userId)
        => _dataTypeService.CreateContainer(parentId, Guid.NewGuid(), name, userId);

    protected override Attempt<OperationResult?> SaveContainer(EntityContainer container, int userId)
        => _dataTypeService.SaveContainer(container, userId);

    protected override Attempt<OperationResult?> DeleteContainer(int containerId, int userId)
        => _dataTypeService.DeleteContainer(containerId, userId);
}
