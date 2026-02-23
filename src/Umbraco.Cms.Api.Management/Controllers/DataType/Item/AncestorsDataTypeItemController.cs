using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Services.Entities;
using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;
using Umbraco.Cms.Api.Management.ViewModels.Item;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Controllers.DataType.Item;

[ApiVersion("1.0")]
public class AncestorsDataTypeItemController : DatatypeItemControllerBase
{
    private readonly IItemAncestorService _itemAncestorService;

    public AncestorsDataTypeItemController(IItemAncestorService itemAncestorService)
        => _itemAncestorService = itemAncestorService;

    [HttpGet("ancestors")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<ItemAncestorsResponseModel<DataTypeItemResponseModel>>), StatusCodes.Status200OK)]
    [EndpointSummary("Gets ancestors for a collection of data type items.")]
    [EndpointDescription("Gets the ancestor chains for data type items identified by the provided Ids.")]
    public IActionResult Ancestors(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<ItemAncestorsResponseModel<DataTypeItemResponseModel>>());
        }

        IEnumerable<ItemAncestorsResponseModel<DataTypeItemResponseModel>> result = _itemAncestorService.GetAncestors(
            UmbracoObjectTypes.DataType,
            UmbracoObjectTypes.DataTypeContainer,
            ids,
            ancestors => ancestors.ToDictionary(
                a => a.Key,
                a => new DataTypeItemResponseModel { Id = a.Key, Name = a.Name ?? string.Empty }));

        return Ok(result);
    }
}
