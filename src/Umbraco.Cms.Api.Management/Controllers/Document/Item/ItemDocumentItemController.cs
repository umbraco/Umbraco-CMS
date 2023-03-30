using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;

namespace Umbraco.Cms.Api.Management.Controllers.Document.Item;

public class ItemDocumentItemController : DocumentItemControllerBase
{
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DataTypeItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Item([FromQuery(Name = "key")] Guid[] keys)
    {
        return Ok();
    }
}
