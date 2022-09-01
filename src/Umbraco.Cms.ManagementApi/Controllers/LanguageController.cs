using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.New.Cms.Web.Common.Routing;
using Language = Umbraco.Cms.Core.Models.ContentEditing.Language;

namespace Umbraco.Cms.ManagementApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[BackOfficeRoute("api/v{version:apiVersion}/language")]
public class LanguageController : Controller
{
    private readonly ILocalizationService _localizationService;
    private readonly IUmbracoMapper _umbracoMapper;

    public LanguageController(ILocalizationService localizationService, IUmbracoMapper umbracoMapper)
    {
        _localizationService = localizationService;
        _umbracoMapper = umbracoMapper;
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
    [ProducesResponseType(typeof(IDictionary<string, string>), StatusCodes.Status200OK)]
    public async Task<IEnumerable<Language>?> GetAllLanguages()
    {
        IEnumerable<ILanguage> allLanguages = _localizationService.GetAllLanguages();

        return _umbracoMapper.Map<IEnumerable<ILanguage>, IEnumerable<Language>>(allLanguages);
    }

    [HttpGet("getLanguage")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Language?>> GetLanguage(int id)
    {
        ILanguage? lang = _localizationService.GetLanguageById(id);
        if (lang == null)
        {
            return NotFound();
        }

        return _umbracoMapper.Map<Language>(lang);
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
    [Authorize(Policy = AuthorizationPolicies.TreeAccessLanguages)]
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
    // [Authorize(Policy = AuthorizationPolicies.TreeAccessLanguages)]
    public async Task<ActionResult<Language?>> SaveLanguage(Language language)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        // this is prone to race conditions but the service will not let us proceed anyways
        ILanguage? existingByCulture = _localizationService.GetLanguageByIsoCode(language.IsoCode);

        // the localization service might return the generic language even when queried for specific ones (e.g. "da" when queried for "da-DK")
        // - we need to handle that explicitly
        if (existingByCulture?.IsoCode != language.IsoCode)
        {
            existingByCulture = null;
        }

        if (existingByCulture != null && language.Id != existingByCulture.Id)
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

            Core.Models.Language? newLang = _umbracoMapper.Map<Core.Models.Language>(language);

            _localizationService.Save(newLang!);
            return _umbracoMapper.Map<Language>(newLang);
        }

        existingById.IsoCode = language.IsoCode;
        if (!string.IsNullOrEmpty(language.Name))
        {
            existingById.CultureName = language.Name;
        }

        // note that the service will prevent the default language from being "un-defaulted"
        // but does not hurt to test here - though the UI should prevent it too
        if (existingById.IsDefault && !language.IsDefault)
        {
            ModelState.AddModelError("IsDefault", "Cannot un-default the default language.");
            return ValidationProblem(ModelState);
        }

        existingById.IsDefault = language.IsDefault;
        existingById.IsMandatory = language.IsMandatory;
        existingById.FallbackLanguageId = language.FallbackLanguageId;

        // modifying an existing language can create a fallback, verify
        // note that the service will check again, dealing with race conditions
        if (existingById.FallbackLanguageId.HasValue)
        {
            var languages = _localizationService.GetAllLanguages().ToDictionary(x => x.Id, x => x);
            if (!languages.ContainsKey(existingById.FallbackLanguageId.Value))
            {
                ModelState.AddModelError("FallbackLanguage", "The selected fall back language does not exist.");
                return ValidationProblem(ModelState);
            }

            if (CreatesCycle(existingById, languages))
            {
                ModelState.AddModelError("FallbackLanguage",
                    $"The selected fall back language {languages[existingById.FallbackLanguageId.Value].IsoCode} would create a circular path.");
                return ValidationProblem(ModelState);
            }
        }

        _localizationService.Save(existingById);
        return _umbracoMapper.Map<Language>(existingById);
    }

    // see LocalizationService
    private bool CreatesCycle(ILanguage language, IDictionary<int, ILanguage> languages)
    {
        // a new language is not referenced yet, so cannot be part of a cycle
        if (!language.HasIdentity)
        {
            return false;
        }

        var id = language.FallbackLanguageId;
        while (true) // assuming languages does not already contains a cycle, this must end
        {
            if (!id.HasValue)
            {
                return false; // no fallback means no cycle
            }

            if (id.Value == language.Id)
            {
                return true; // back to language = cycle!
            }

            id = languages[id.Value].FallbackLanguageId; // else keep chaining
        }
    }
}
