using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Dictionary;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

public class AllDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IUmbracoMapper _umbracoMapper;

    public AllDictionaryController(ILocalizationService localizationService, IUmbracoMapper umbracoMapper)
    {
        _localizationService = localizationService;
        _umbracoMapper = umbracoMapper;
    }


    /// <summary>
    ///     Retrieves a list with all dictionary items
    /// </summary>
    /// <returns>
    ///     The <see cref="IEnumerable{T}" />.
    /// </returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(PagedViewModel<DictionaryOverviewViewModel>), StatusCodes.Status200OK)]
    public async Task<PagedViewModel<DictionaryOverviewViewModel>> All(int skip, int take)
    {
        IDictionaryItem[] items = _localizationService.GetDictionaryItemDescendants(null).ToArray();
        var list = new List<DictionaryOverviewViewModel>(items.Length);

        // Build the proper tree structure, as we can have nested dictionary items
        BuildTree(list, items);

        var model = new PagedViewModel<DictionaryOverviewViewModel>
        {
            Total = list.Count,
            Items = list.Skip(skip).Take(take),
        };
        return await Task.FromResult(model);
    }

    // recursive method to build a tree structure from the flat structure returned above
    private void BuildTree(List<DictionaryOverviewViewModel> list, IDictionaryItem[] items, int level = 0, Guid? parentId = null)
    {
        IDictionaryItem[] children = items.Where(t => t.ParentId == parentId).ToArray();
        if (children.Any() == false)
        {
            return;
        }

        foreach (IDictionaryItem child in children.OrderBy(item => item.ItemKey))
        {
            DictionaryOverviewViewModel? display = _umbracoMapper.Map<IDictionaryItem, DictionaryOverviewViewModel>(child);
            if (display is not null)
            {
                display.Level = level;
                list.Add(display);
            }

            BuildTree(list, items, level + 1, child.Key);
        }
    }
}
