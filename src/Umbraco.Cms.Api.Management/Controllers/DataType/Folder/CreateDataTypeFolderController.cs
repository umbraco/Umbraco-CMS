using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Folder;

public class CreateDataTypeFolderController : DataTypeFolderControllerBase
{
    public CreateDataTypeFolderController(IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IDataTypeService dataTypeService)
        : base(backOfficeSecurityAccessor, dataTypeService)
    {
    }

    [HttpPost]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult> Create(FolderCreateModel folderCreateModel)
        => await Task.FromResult(CreateFolder<ByKeyDataTypeFolderController>(folderCreateModel, controller => nameof(controller.ByKey)));
}
