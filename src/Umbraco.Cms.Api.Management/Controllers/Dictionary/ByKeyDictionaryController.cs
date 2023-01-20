using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Api.Management.ViewModels.Dictionary;

namespace Umbraco.Cms.Api.Management.Controllers.Dictionary;

public class ByKeyDictionaryController : DictionaryControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly IDictionaryFactory _dictionaryFactory;

    public ByKeyDictionaryController(ILocalizationService localizationService, IDictionaryFactory dictionaryFactory)
    {
        _localizationService = localizationService;
        _dictionaryFactory = dictionaryFactory;
    }

    [HttpGet($"{{{nameof(key)}:guid}}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(DictionaryItemViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DictionaryItemViewModel>> ByKey(Guid key)
    {
        IDictionaryItem? dictionary = _localizationService.GetDictionaryItemById(key);
        if (dictionary == null)
        {
            return NotFound();
        }

        return await Task.FromResult(Ok(_dictionaryFactory.CreateDictionaryItemViewModel(dictionary)));
    }
}
