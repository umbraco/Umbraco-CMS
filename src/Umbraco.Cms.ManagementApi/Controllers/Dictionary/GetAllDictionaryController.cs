using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Pagination;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

//TODO: Implement this when the PagedViewModel gets merged into dev
public class GetAllDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IUmbracoMapper _umbracoMapper;

    public GetAllDictionaryController(ILocalizationService localizationService, IUmbracoMapper umbracoMapper)
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
    [ProducesResponseType(typeof(IActionResult), StatusCodes.Status200OK)]
    public PagedViewModel<DictionaryOverviewDisplay> GetAll(int skip, int take)
    {
        IDictionaryItem[] items = _localizationService.GetDictionaryItemDescendants(null).ToArray();
        var list = new List<DictionaryOverviewDisplay>(items.Length);

        BuildTree(list, items);

        var model = new PagedViewModel<DictionaryOverviewDisplay>
        {
            Total = list.Count,
            Items = list.Skip(skip).Take(take),
        };
        return model;
    }

    // recursive method to build a tree structure from the flat structure returned above
    private void BuildTree(List<DictionaryOverviewDisplay> list, IDictionaryItem[] items, int level = 0, Guid? parentId = null)
    {
        IDictionaryItem[] children = items.Where(t => t.ParentId == parentId).ToArray();
        if (children.Any() == false)
        {
            return;
        }

        foreach (IDictionaryItem child in children.OrderBy(item => item.ItemKey))
        {
            DictionaryOverviewDisplay? display = _umbracoMapper.Map<IDictionaryItem, DictionaryOverviewDisplay>(child);
            if (display is not null)
            {
                display.Level = level;
                list.Add(display);
            }

            BuildTree(list, items, level + 1, child.Key);
        }
    }
}
