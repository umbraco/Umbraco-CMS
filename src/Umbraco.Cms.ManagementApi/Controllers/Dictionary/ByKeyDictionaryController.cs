using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Dictionary;
using Umbraco.New.Cms.Core.Factories;

namespace Umbraco.Cms.ManagementApi.Controllers.Dictionary;

public class ByIdDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IDictionaryFactory _dictionaryFactory;

    public ByIdDictionaryController(
        ILocalizationService localizationService,
        IDictionaryFactory dictionaryFactory)
    {
        _localizationService = localizationService;
        _dictionaryFactory = dictionaryFactory;
    }

    /// <summary>
    ///     Gets a dictionary item by guid
    /// </summary>
    /// <param name="key">
    ///     The id.
    /// </param>
    /// <returns>
    ///     The <see cref="DictionaryDisplay" />. Returns a not found response when dictionary item does not exist
    /// </returns>
    [HttpGet("{key:guid}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DictionaryViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DictionaryViewModel>> ByKey(Guid key)
    {
        IDictionaryItem? dictionary = _localizationService.GetDictionaryItemById(key);
        if (dictionary == null)
        {
            return NotFound();
        }

        return await Task.FromResult(_dictionaryFactory.CreateDictionaryViewModel(dictionary));
    }
}
