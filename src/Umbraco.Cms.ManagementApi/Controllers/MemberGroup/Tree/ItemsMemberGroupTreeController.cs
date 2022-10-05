using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Tree;

namespace Umbraco.Cms.ManagementApi.Controllers.MemberGroup.Tree;

public class ItemsMemberGroupTreeController : MemberGroupTreeControllerBase
{
    public ItemsMemberGroupTreeController(IEntityService entityService)
        : base(entityService)
    {
    }

    [HttpGet("items")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<EntityTreeItemViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EntityTreeItemViewModel>>> Items([FromQuery(Name = "key")] Guid[] keys)
        => await GetItems(keys);
}
