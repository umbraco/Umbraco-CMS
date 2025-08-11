using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;
using Umbraco.Cms.Api.Management.ViewModels.Tree;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Tree;

public class SiblingsDataTypeTreeController : DataTypeTreeControllerBase
{
    public SiblingsDataTypeTreeController(IEntityService entityService, IDataTypeService dataTypeService)
        : base(entityService, dataTypeService)
    {
    }

    [HttpGet("siblings")]
    [ProducesResponseType(typeof(SubsetViewModel<DataTypeTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<SubsetViewModel<DataTypeTreeItemResponseModel>>> Siblings(CancellationToken cancellationToken, Guid target, int before, int after, bool foldersOnly = false)
    {
        RenderFoldersOnly(foldersOnly);
        return await GetSiblings(target, before, after);
    }
}
