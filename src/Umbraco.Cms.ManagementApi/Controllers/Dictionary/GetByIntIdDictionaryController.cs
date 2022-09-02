using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

[ApiVersion("1.0")]
public class GetByIntIdDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IUmbracoMapper _umbracoMapper;

    public GetByIntIdDictionaryController(ILocalizationService localizationService, IUmbracoMapper umbracoMapper)
    {
        _localizationService = localizationService;
        _umbracoMapper = umbracoMapper;
    }

    /// <summary>
    ///     Gets a dictionary item by id
    /// </summary>
    /// <param name="id">
    ///     The id.
    /// </param>
    /// <returns>
    ///     The <see cref="DictionaryDisplay" />. Returns a not found response when dictionary item does not exist
    /// </returns>
    public ActionResult<DictionaryDisplay?> GetById(int id)
    {
        IDictionaryItem? dictionary = _localizationService.GetDictionaryItemById(id);
        if (dictionary == null)
        {
            return NotFound();
        }

        return _umbracoMapper.Map<IDictionaryItem, DictionaryDisplay>(dictionary);
    }
}
