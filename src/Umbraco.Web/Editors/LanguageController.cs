using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Language = Umbraco.Web.Models.ContentEditing.Language;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Backoffice controller supporting the dashboard for language administration.
    /// </summary>
    [PluginController("UmbracoApi")]
    [PrefixlessBodyModelValidator]
    public class LanguageController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Returns all cultures available for creating languages.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IDictionary<string, string> GetAllCultures()
        {
            return
                CultureInfo.GetCultures(CultureTypes.AllCultures)
                    .Where(x => !x.Name.IsNullOrWhiteSpace())
                    .OrderBy(x => x.DisplayName).ToDictionary(x => x.Name, x => x.DisplayName);
        }

        /// <summary>
        /// Returns all currently configured languages.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Language> GetAllLanguages()
        {
            var allLanguages = Services.LocalizationService.GetAllLanguages();

            return Mapper.Map<IEnumerable<ILanguage>, IEnumerable<Language>>(allLanguages);
        }

        [HttpGet]
        public Language GetLanguage(int id)
        {
            var lang = Services.LocalizationService.GetLanguageById(id);
            if (lang == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            var model = Mapper.Map<Language>(lang);

            //if there's only one language, by default it is the default
            var allLangs = Services.LocalizationService.GetAllLanguages().OrderBy(x => x.Id).ToList();
            if (!lang.IsDefault)
            {
                if (allLangs.Count == 1)
                {
                    model.IsDefault = true;
                    model.IsMandatory = true;
                }   
                else if (allLangs.All(x => !x.IsDefault))
                {
                    //if no language has the default flag, then the default language is the one with the lowest id
                    model.IsDefault = allLangs[0].Id == lang.Id;
                    model.IsMandatory = allLangs[0].Id == lang.Id;
                }
            }

            return model;
        }

        /// <summary>
        /// Deletes a language with a given ID
        /// </summary>
        [UmbracoTreeAuthorize(Core.Constants.Trees.Languages)]
        [HttpDelete]
        [HttpPost]
        public IHttpActionResult DeleteLanguage(int id)
        {
            var language = Services.LocalizationService.GetLanguageById(id);
            if (language == null)
            {
                return NotFound();
            }

            var langs = Services.LocalizationService.GetAllLanguages().ToArray();
            var totalLangs = langs.Length;

            if (language.IsDefault || totalLangs == 1)
            {
                var message = $"Language '{language.CultureName}' is currently set to 'default' or it is the only installed language and cannot be deleted.";
                throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse(message));
            }

            if (langs.Any(x => x.FallbackLanguageId.HasValue && x.FallbackLanguageId.Value == language.Id))
            {
                var message = $"Language '{language.CultureName}' is defined as a fall-back language for one or more other languages, and so cannot be deleted.";
                throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse(message));
            }

            Services.LocalizationService.Delete(language);

            return Ok();
        }

        /// <summary>
        /// Creates or saves a language
        /// </summary>
        [UmbracoTreeAuthorize(Core.Constants.Trees.Languages)]
        [HttpPost]
        public Language SaveLanguage(Language language)
        {
            if (!ModelState.IsValid)
                throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));

            var found = Services.LocalizationService.GetLanguageByIsoCode(language.IsoCode);

            if (found != null && language.Id != found.Id)
            {
                //someone is trying to create a language that alraedy exist
                ModelState.AddModelError("IsoCode", "The language " + language.IsoCode + " already exists");
                throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
            }

            if (found == null)
            {
                CultureInfo culture;
                try
                {
                    culture = CultureInfo.GetCultureInfo(language.IsoCode);
                }
                catch (CultureNotFoundException)
                {
                    ModelState.AddModelError("IsoCode", "No Culture found with name " + language.IsoCode);
                    throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
                }

                //create it
                var newLang = new Core.Models.Language(culture.Name)
                {
                    CultureName = culture.DisplayName,
                    IsDefault = language.IsDefault,
                    IsMandatory = language.IsMandatory,
                    FallbackLanguageId = language.FallbackLanguageId
                };

                Services.LocalizationService.Save(newLang);
                return Mapper.Map<Language>(newLang);
            }

            found.IsMandatory = language.IsMandatory;
            found.IsDefault = language.IsDefault;
            found.FallbackLanguageId = language.FallbackLanguageId;

            string selectedFallbackLanguageCultureName;
            if (DoesUpdatedFallbackLanguageCreateACircularPath(found, out selectedFallbackLanguageCultureName))
            {
                ModelState.AddModelError("FallbackLanguage", "The selected fall back language '" + selectedFallbackLanguageCultureName + "' would create a circular path.");
                throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
            }

            Services.LocalizationService.Save(found);
            return Mapper.Map<Language>(found);
        }

        private bool DoesUpdatedFallbackLanguageCreateACircularPath(ILanguage language, out string selectedFallbackLanguageCultureName)
        {
            if (language.FallbackLanguageId.HasValue == false)
            {
                selectedFallbackLanguageCultureName = string.Empty;
                return false;
            }

            var languages = Services.LocalizationService.GetAllLanguages().ToArray();
            var fallbackLanguageId = language.FallbackLanguageId;
            while (fallbackLanguageId.HasValue)
            {
                if (fallbackLanguageId.Value == language.Id)
                {
                    // We've found the current language in the path of fall back languages, so we have a circular path.
                    selectedFallbackLanguageCultureName = GetLanguageFromCollectionById(languages, fallbackLanguageId.Value).CultureName;
                    return true;
                }

                fallbackLanguageId = GetLanguageFromCollectionById(languages, fallbackLanguageId.Value).FallbackLanguageId;
            }

            selectedFallbackLanguageCultureName = string.Empty;
            return false;
        }

        private static ILanguage GetLanguageFromCollectionById(IEnumerable<ILanguage> languages, int id)
        {
            return languages.Single(x => x.Id == id);
        }
    }
}
