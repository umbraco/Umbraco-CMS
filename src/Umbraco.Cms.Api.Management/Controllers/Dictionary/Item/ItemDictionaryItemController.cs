using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary.Item;

[ApiVersion("1.0")]
public class ItemDictionaryItemController : DictionaryItemControllerBase
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IUmbracoMapper _mapper;

    public ItemDictionaryItemController(IDictionaryItemService dictionaryItemService, IUmbracoMapper mapper)
    {
        _dictionaryItemService = dictionaryItemService;
        _mapper = mapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<DictionaryItemItemResponseModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Item(
        CancellationToken cancellationToken,
        [FromQuery(Name = "id")] HashSet<Guid> ids)
    {
        if (ids.Count is 0)
        {
            return Ok(Enumerable.Empty<DictionaryItemItemResponseModel>());
        }

        IEnumerable<IDictionaryItem> dictionaryItems = await _dictionaryItemService.GetManyAsync(ids.ToArray());

        List<DictionaryItemItemResponseModel> responseModels = _mapper.MapEnumerable<IDictionaryItem, DictionaryItemItemResponseModel>(dictionaryItems);
        return Ok(responseModels);
    }
}
