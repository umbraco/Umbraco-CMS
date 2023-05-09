using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Folder;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Folder;

[ApiVersion("1.0")]
public class ByKeyDataTypeFolderController : DataTypeFolderControllerBase
{
    public ByKeyDataTypeFolderController(IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IDataTypeContainerService dataTypeContainerService)
        : base(backOfficeSecurityAccessor, dataTypeContainerService)
    {
    }

    [HttpGet("{id:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(FolderReponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ByKey(Guid id) => await GetFolderAsync(id);
}
