using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.ManagementApi.ViewModels.Languages;
using Umbraco.New.Cms.Core.Services.Installer;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[BackOfficeRoute("api/v{version:apiVersion}/language")]
public class LanguageController : Controller
{
    private readonly ILocalizationService _localizationService;
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly ILanguageService _languageService;

    public LanguageController(ILocalizationService localizationService, IUmbracoMapper umbracoMapper, ILanguageService languageService)
    {
        _localizationService = localizationService;
        _umbracoMapper = umbracoMapper;
        _languageService = languageService;
    }

    /// <summary>
    ///     Returns all cultures available for creating languages.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getAllCultures")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IDictionary<string, string>), StatusCodes.Status200OK)]
    public IDictionary<string, string> GetAllCultures()
        => CultureInfo.GetCultures(CultureTypes.AllCultures).DistinctBy(x => x.Name).OrderBy(x => x.EnglishName)
            .ToDictionary(x => x.Name, x => x.EnglishName);

    /// <summary>
    ///     Returns all currently configured languages.
    /// </summary>
    /// <returns></returns>
    [HttpGet("getAllLanguages")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(IEnumerable<LanguageViewModel>), StatusCodes.Status200OK)]
    public async Task<IEnumerable<LanguageViewModel>?> GetAllLanguages()
    {
        IEnumerable<ILanguage> allLanguages = _localizationService.GetAllLanguages();

        return _umbracoMapper.Map<IEnumerable<ILanguage>, IEnumerable<LanguageViewModel>>(allLanguages);
    }

    [HttpGet("getLanguage")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LanguageViewModel?>> GetLanguage(int id)
    {
        ILanguage? lang = _localizationService.GetLanguageById(id);
        if (lang == null)
        {
            return NotFound();
        }

        return _umbracoMapper.Map<LanguageViewModel>(lang);
    }

    /// <summary>
    ///     Deletes a language with a given ID
    /// </summary>
    [HttpPost("deleteLanguage")]
    [HttpDelete("deleteLanguage")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    // TODO: This needs to be an authorized endpoint.
    public async Task<IActionResult> DeleteLanguage(int id)
    {
        ILanguage? language = _localizationService.GetLanguageById(id);
        if (language == null)
        {
            return NotFound();
        }

        // the service would not let us do it, but test here nevertheless
        if (language.IsDefault)
        {
            var invalidModelProblem = new ProblemDetails
            {
                Title = "Cannot delete default language",
                Detail = $"Language '{language.IsoCode}' is currently set to 'default' and can not be deleted.",
                Status = StatusCodes.Status400BadRequest,
                Type = "Error",
            };
            return BadRequest(invalidModelProblem);
        }

        // service is happy deleting a language that's fallback for another language,
        // will just remove it - so no need to check here
        _localizationService.Delete(language);

        return Ok();
    }

    /// <summary>
    ///     Creates or saves a language
    /// </summary>
    [HttpPost("saveLanguage")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    // TODO: This needs to be an authorized endpoint.
    public async Task<ActionResult<LanguageViewModel?>> SaveLanguage(LanguageViewModel language)
    {
        if (_languageService.LanguageAlreadyExists(language.Id, language.IsoCode))
        {
            // Someone is trying to create a language that already exist
            ModelState.AddModelError("IsoCode", "The language " + language.IsoCode + " already exists");
            return ValidationProblem(ModelState);
        }

        ILanguage? existingById = language.Id != default ? _localizationService.GetLanguageById(language.Id) : null;
        if (existingById == null)
        {
            // Creating a new lang...
            CultureInfo culture;
            try
            {
                culture = CultureInfo.GetCultureInfo(language.IsoCode!);
            }
            catch (CultureNotFoundException)
            {
                ModelState.AddModelError("IsoCode", "No Culture found with name " + language.IsoCode);
                return ValidationProblem(ModelState);
            }

            language.Name ??= culture.EnglishName;

            Language? newLang = _umbracoMapper.Map<Language>(language);

            _localizationService.Save(newLang!);
            return _umbracoMapper.Map<LanguageViewModel>(newLang);
        }

        // note that the service will prevent the default language from being "un-defaulted"
        // but does not hurt to test here - though the UI should prevent it too
        if (existingById.IsDefault && !language.IsDefault)
        {
            ModelState.AddModelError("IsDefault", "Cannot un-default the default language.");
            return ValidationProblem(ModelState);
        }

        existingById = _umbracoMapper.Map(language, existingById);

        if (!_languageService.CanUseLanguagesFallbackLanguage(existingById))
        {
            ModelState.AddModelError("FallbackLanguage", "The selected fall back language does not exist.");
            return ValidationProblem(ModelState);
        }

        if (!_languageService.CanGetProperFallbackLanguage(existingById))
        {
            ModelState.AddModelError("FallbackLanguage", $"The selected fall back language {_localizationService.GetLanguageById(existingById.FallbackLanguageId!.Value)} would create a circular path.");
            return ValidationProblem(ModelState);
        }

        _localizationService.Save(existingById);
        return _umbracoMapper.Map<LanguageViewModel>(existingById);
    }
}
