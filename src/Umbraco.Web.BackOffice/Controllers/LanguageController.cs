﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Editors;
using Language = Umbraco.Web.Models.ContentEditing.Language;

namespace Umbraco.Web.BackOffice.Controllers
{
    /// <summary>
    /// Backoffice controller supporting the dashboard for language administration.
    /// </summary>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    //[PrefixlessBodyModelValidator]
    public class LanguageController : UmbracoAuthorizedJsonController
    {
        private readonly ILocalizationService _localizationService;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly GlobalSettings _globalSettings;

        public LanguageController(ILocalizationService localizationService,
            UmbracoMapper umbracoMapper,
            IOptions<GlobalSettings> globalSettings)
        {
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
            _globalSettings = globalSettings.Value ?? throw new ArgumentNullException(nameof(globalSettings));
        }

        /// <summary>
        /// Returns all cultures available for creating languages.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IDictionary<string, string> GetAllCultures()
        {
            // get cultures - new-ing instances to get proper display name,
            // in the current culture, and not the cached one
            // (see notes in Language class about culture info names)
            return CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Where(x => !x.Name.IsNullOrWhiteSpace())
                .Select(x => new CultureInfo(x.Name)) // important!
                .OrderBy(x => x.DisplayName)
                .ToDictionary(x => x.Name, x => x.DisplayName);
        }

        /// <summary>
        /// Returns all currently configured languages.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Language> GetAllLanguages()
        {
            var allLanguages = _localizationService.GetAllLanguages();

            return _umbracoMapper.Map<IEnumerable<ILanguage>, IEnumerable<Language>>(allLanguages);
        }

        [HttpGet]
        public ActionResult<Language> GetLanguage(int id)
        {
            var lang = _localizationService.GetLanguageById(id);
            if (lang == null)
                return NotFound();

            return _umbracoMapper.Map<Language>(lang);
        }

        /// <summary>
        /// Deletes a language with a given ID
        /// </summary>
        [UmbracoTreeAuthorize(Constants.Trees.Languages)]
        [HttpDelete]
        [HttpPost]
        public IActionResult DeleteLanguage(int id)
        {
            var language = _localizationService.GetLanguageById(id);
            if (language == null)
            {
                return NotFound();
            }

            // the service would not let us do it, but test here nevertheless
            if (language.IsDefault)
            {
                var message = $"Language '{language.IsoCode}' is currently set to 'default' and can not be deleted.";
                throw HttpResponseException.CreateNotificationValidationErrorResponse(message);
            }

            // service is happy deleting a language that's fallback for another language,
            // will just remove it - so no need to check here

            _localizationService.Delete(language);

            return Ok();
        }

        /// <summary>
        /// Creates or saves a language
        /// </summary>
        [UmbracoTreeAuthorize(Constants.Trees.Languages)]
        [HttpPost]
        public Language SaveLanguage(Language language)
        {
            if (!ModelState.IsValid)
                throw HttpResponseException.CreateValidationErrorResponse(ModelState);

            // this is prone to race conditions but the service will not let us proceed anyways
            var existingByCulture = _localizationService.GetLanguageByIsoCode(language.IsoCode);

            // the localization service might return the generic language even when queried for specific ones (e.g. "da" when queried for "da-DK")
            // - we need to handle that explicitly
            if (existingByCulture?.IsoCode != language.IsoCode)
            {
                existingByCulture = null;
            }

            if (existingByCulture != null && language.Id != existingByCulture.Id)
            {
                //someone is trying to create a language that already exist
                ModelState.AddModelError("IsoCode", "The language " + language.IsoCode + " already exists");
                throw HttpResponseException.CreateValidationErrorResponse(ModelState);
            }

            var existingById = language.Id != default ? _localizationService.GetLanguageById(language.Id) : null;

            if (existingById == null)
            {
                //Creating a new lang...

                CultureInfo culture;
                try
                {
                    culture = CultureInfo.GetCultureInfo(language.IsoCode);
                }
                catch (CultureNotFoundException)
                {
                    ModelState.AddModelError("IsoCode", "No Culture found with name " + language.IsoCode);
                    throw HttpResponseException.CreateValidationErrorResponse(ModelState);
                }

                // create it (creating a new language cannot create a fallback cycle)
                var newLang = new Core.Models.Language(_globalSettings, culture.Name)
                {
                    CultureName = culture.DisplayName,
                    IsDefault = language.IsDefault,
                    IsMandatory = language.IsMandatory,
                    FallbackLanguageId = language.FallbackLanguageId
                };

                _localizationService.Save(newLang);
                return _umbracoMapper.Map<Language>(newLang);
            }

            existingById.IsMandatory = language.IsMandatory;

            // note that the service will prevent the default language from being "un-defaulted"
            // but does not hurt to test here - though the UI should prevent it too
            if (existingById.IsDefault && !language.IsDefault)
            {
                ModelState.AddModelError("IsDefault", "Cannot un-default the default language.");
                throw HttpResponseException.CreateValidationErrorResponse(ModelState);
            }

            existingById.IsDefault = language.IsDefault;
            existingById.FallbackLanguageId = language.FallbackLanguageId;
            existingById.IsoCode = language.IsoCode;

            // modifying an existing language can create a fallback, verify
            // note that the service will check again, dealing with race conditions
            if (existingById.FallbackLanguageId.HasValue)
            {
                var languages = _localizationService.GetAllLanguages().ToDictionary(x => x.Id, x => x);
                if (!languages.ContainsKey(existingById.FallbackLanguageId.Value))
                {
                    ModelState.AddModelError("FallbackLanguage", "The selected fall back language does not exist.");
                    throw HttpResponseException.CreateValidationErrorResponse(ModelState);
                }
                if (CreatesCycle(existingById, languages))
                {
                    ModelState.AddModelError("FallbackLanguage", $"The selected fall back language {languages[existingById.FallbackLanguageId.Value].IsoCode} would create a circular path.");
                    throw HttpResponseException.CreateValidationErrorResponse(ModelState);
                }
            }

            _localizationService.Save(existingById);
            return _umbracoMapper.Map<Language>(existingById);
        }

        // see LocalizationService
        private bool CreatesCycle(ILanguage language, IDictionary<int, ILanguage> languages)
        {
            // a new language is not referenced yet, so cannot be part of a cycle
            if (!language.HasIdentity) return false;

            var id = language.FallbackLanguageId;
            while (true) // assuming languages does not already contains a cycle, this must end
            {
                if (!id.HasValue) return false; // no fallback means no cycle
                if (id.Value == language.Id) return true; // back to language = cycle!
                id = languages[id.Value].FallbackLanguageId; // else keep chaining
            }
        }
    }
}
