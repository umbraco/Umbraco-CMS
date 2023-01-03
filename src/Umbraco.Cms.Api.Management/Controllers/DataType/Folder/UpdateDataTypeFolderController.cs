using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Folder;

public class UpdateDataTypeFolderController : DataTypeFolderControllerBase
{
    public UpdateDataTypeFolderController(IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IDataTypeService dataTypeService)
        : base(backOfficeSecurityAccessor, dataTypeService)
    {
    }

    [HttpPut("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Update(Guid key, FolderUpdateModel folderUpdateModel)
        => await Task.FromResult(UpdateFolder(key, folderUpdateModel));
}
