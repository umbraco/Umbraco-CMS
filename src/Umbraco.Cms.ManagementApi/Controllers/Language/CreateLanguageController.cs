using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Language;
using Umbraco.New.Cms.Core.Services.Installer;

namespace Umbraco.Cms.ManagementApi.Controllers.Language;

public class CreateLanguageController : LanguageControllerBase
{
    private readonly ILanguageService _languageService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly ILocalizationService _localizationService;

    public CreateLanguageController(ILanguageService languageService, IUmbracoMapper umbracoMapper, ILocalizationService localizationService)
    {
        _languageService = languageService;
        _umbracoMapper = umbracoMapper;
        _localizationService = localizationService;
    }

    /// <summary>
    ///     Creates or saves a language
    /// </summary>
    [HttpPost("create")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    // TODO: This needs to be an authorized endpoint.
    public async Task<ActionResult> Create(LanguageViewModel language)
    {
        if (_languageService.LanguageAlreadyExists(language.Id, language.IsoCode))
        {
            // Someone is trying to create a language that already exist
            ModelState.AddModelError("IsoCode", "The language " + language.IsoCode + " already exists");
            return ValidationProblem(ModelState);
        }

        // Creating a new lang...
        CultureInfo culture;
        try
        {
            culture = CultureInfo.GetCultureInfo(language.IsoCode);
        }
        catch (CultureNotFoundException)
        {
            ModelState.AddModelError("IsoCode", "No Culture found with name " + language.IsoCode);
            return ValidationProblem(ModelState);
        }

        language.Name ??= culture.EnglishName;

        ILanguage newLang = _umbracoMapper.Map<ILanguage>(language)!;

        _localizationService.Save(newLang);
        return await Task.FromResult(Created($"api/v1.0/language/{newLang.Id}", null));
    }
}
