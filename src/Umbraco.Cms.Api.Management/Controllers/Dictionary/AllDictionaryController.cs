using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;
using Umbraco.Cms.Api.Common.ViewModels.Pagination;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

public class AllDictionaryController : DictionaryControllerBase
{
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllDictionaryController(IDictionaryItemService dictionaryItemService, IUmbracoMapper umbracoMapper)
    {
        _dictionaryItemService = dictionaryItemService;
        _umbracoMapper = umbracoMapper;
    }

    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DictionaryOverviewViewModel>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedViewModel<DictionaryOverviewViewModel>>> All(int skip = 0, int take = 100)
    {
        // unfortunately we can't paginate here...we'll have to get all and paginate in memory
        IDictionaryItem[] items = (await _dictionaryItemService.GetDescendantsAsync(null)).ToArray();
        var model = new PagedViewModel<DictionaryOverviewViewModel>
        {
            Total = items.Length,
            Items = _umbracoMapper.MapEnumerable<IDictionaryItem, DictionaryOverviewViewModel>(items.Skip(skip).Take(take))
        };
        return await Task.FromResult(model);
    }
}
