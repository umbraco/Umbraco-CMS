using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Item;

public class ItemDictionaryItemController : DictionaryItemControllerBase
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IUmbracoMapper _mapper;

    public ItemDictionaryItemController(IDictionaryItemService dictionaryItemService, IUmbracoMapper mapper)
    {
        _dictionaryItemService = dictionaryItemService;
        _mapper = mapper;
    }

    [HttpGet("item")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DictionaryItemItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Item([FromQuery(Name = "key")] SortedSet<Guid> keys)
    {
        IEnumerable<IDictionaryItem> dictionaryItems = await _dictionaryItemService.GetManyAsync(keys.ToArray());

        List<DictionaryItemItemResponseModel> responseModels = _mapper.MapEnumerable<IDictionaryItem, DictionaryItemItemResponseModel>(dictionaryItems);
        return Ok(responseModels);
    }
}
