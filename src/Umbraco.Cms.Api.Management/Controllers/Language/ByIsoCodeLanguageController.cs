using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Language;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Controllers.Language;

public class ByIsoCodeLanguageController : LanguageControllerBase
{
    private readonly ILocalizationService _localizationService;
    private readonly ILanguageFactory _languageFactory;

    public ByIsoCodeLanguageController(ILocalizationService localizationService, ILanguageFactory languageFactory)
    {
        _localizationService = localizationService;
        _languageFactory = languageFactory;
    }

    [HttpGet("{isoCode}")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(LanguageViewModel), StatusCodes.Status200OK)]
    public async Task<ActionResult<LanguageViewModel>> ByIsoCode(string isoCode)
    {
        ILanguage? language = _localizationService.GetLanguageByIsoCode(isoCode);
        if (language == null)
        {
            return NotFound();
        }

        return await Task.FromResult(Ok(_languageFactory.CreateLanguageViewModel(language)));
    }
}
