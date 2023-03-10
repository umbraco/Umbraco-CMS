using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Tree;

namespace Umbraco.Cms.Api.Management.Controllers.MemberGroup.Tree;

public class ItemsMemberGroupTreeController : MemberGroupTreeControllerBase
{
    public ItemsMemberGroupTreeController(IEntityService entityService)
        : base(entityService)
    {
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<EntityTreeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EntityTreeItemResponseModel>>> Items([FromQuery(Name = "key")] Guid[] keys)
        => await GetItems(keys);
}
