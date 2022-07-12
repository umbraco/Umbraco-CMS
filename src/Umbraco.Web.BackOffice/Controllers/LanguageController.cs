using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Language = Umbraco.Cms.Core.Models.ContentEditing.Language;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

/// <summary>
///     Backoffice controller supporting the dashboard for language administration.
/// </summary>
[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class LanguageController : UmbracoAuthorizedJsonController
{
    private readonly ILocalizationService _localizationService;
    private readonly IUmbracoMapper _umbracoMapper;

    [ActivatorUtilitiesConstructor]
    public LanguageController(ILocalizationService localizationService, IUmbracoMapper umbracoMapper)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
    }

    [Obsolete("Use the constructor without global settings instead, scheduled for removal in V11.")]
    public LanguageController(ILocalizationService localizationService, IUmbracoMapper umbracoMapper,
        IOptionsSnapshot<GlobalSettings> globalSettings)
        : this(localizationService, umbracoMapper)
    {
    }

    /// <summary>
    ///     Returns all cultures available for creating languages.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IDictionary<string, string> GetAllCultures()
        => CultureInfo.GetCultures(CultureTypes.AllCultures).DistinctBy(x => x.Name).OrderBy(x => x.EnglishName)
            .ToDictionary(x => x.Name, x => x.EnglishName);

    /// <summary>
    ///     Returns all currently configured languages.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IEnumerable<Language>? GetAllLanguages()
    {
        IEnumerable<ILanguage> allLanguages = _localizationService.GetAllLanguages();

        return _umbracoMapper.Map<IEnumerable<ILanguage>, IEnumerable<Language>>(allLanguages);
    }

    [HttpGet]
    public ActionResult<Language?> GetLanguage(int id)
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
    [Authorize(Policy = AuthorizationPolicies.TreeAccessLanguages)]
    [HttpDelete]
    [HttpPost]
    public IActionResult DeleteLanguage(int id)
    {
        ILanguage? language = _localizationService.GetLanguageById(id);
        if (language == null)
        {
            return NotFound();
        }

        // the service would not let us do it, but test here nevertheless
        if (language.IsDefault)
        {
            var message = $"Language '{language.IsoCode}' is currently set to 'default' and can not be deleted.";
            return ValidationProblem(message);
        }

        // service is happy deleting a language that's fallback for another language,
        // will just remove it - so no need to check here
        _localizationService.Delete(language);

        return Ok();
    }

    /// <summary>
    ///     Creates or saves a language
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.TreeAccessLanguages)]
    [HttpPost]
    public ActionResult<Language?> SaveLanguage(Language language)
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

            // create it (creating a new language cannot create a fallback cycle)
            var newLang = new Core.Models.Language(culture.Name, language.Name ?? culture.EnglishName)
            {
                IsDefault = language.IsDefault,
                IsMandatory = language.IsMandatory,
                FallbackLanguageId = language.FallbackLanguageId
            };

            _localizationService.Save(newLang);
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
