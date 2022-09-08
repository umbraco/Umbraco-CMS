using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.DataType.Tree;

public class DataTypeTreeChildrenController : DataTypeTreeControllerBase
{
    public DataTypeTreeChildrenController(IEntityService entityService, IDataTypeService _dataTypeService)
        : base(entityService, _dataTypeService)
    {
    }

    [HttpGet("children")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PagedResult<FolderTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<FolderTreeItemViewModel>>> Children(Guid parentKey, long pageNumber = 0, int pageSize = 100, bool foldersOnly = false)
        => await GetChildren(parentKey, pageNumber, pageSize, foldersOnly);
}
