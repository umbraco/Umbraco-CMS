using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Folder;

public class ByKeyDataTypeFolderController : DataTypeFolderControllerBase
{
    public ByKeyDataTypeFolderController(IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IDataTypeContainerService dataTypeContainerService)
        : base(backOfficeSecurityAccessor, dataTypeContainerService)
    {
    }

    [HttpGet("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FolderViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(Guid key) => await GetFolderAsync(key);
}
