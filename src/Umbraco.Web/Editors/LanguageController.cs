using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
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
            var allLanguages = Services.LocalizationService.GetAllLanguages();

            return Mapper.Map<IEnumerable<ILanguage>, IEnumerable<Language>>(allLanguages);
        }

        [HttpGet]
        public Language GetLanguage(int id)
        {
            var lang = Services.LocalizationService.GetLanguageById(id);
            if (lang == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            return Mapper.Map<Language>(lang);
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

            // the service would not let us do it, but test here nevertheless
            if (language.IsDefault)
            {
                var message = $"Language '{language.IsoCode}' is currently set to 'default' and can not be deleted.";
                throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse(message));
            }

            // service is happy deleting a language that's fallback for another language,
            // will just remove it - so no need to check here

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

            // this is prone to race conditions but the service will not let us proceed anyways
            var existing = Services.LocalizationService.GetLanguageByIsoCode(language.IsoCode);

            // the localization service might return the generic language even when queried for specific ones (e.g. "da" when queried for "da-DK")
            // - we need to handle that explicitly
            if (existing.IsoCode != language.IsoCode)
            {
                existing = null;
            }

            if (existing != null && language.Id != existing.Id)
            {
                //someone is trying to create a language that already exist
                ModelState.AddModelError("IsoCode", "The language " + language.IsoCode + " already exists");
                throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
            }

            if (existing == null)
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

                // create it (creating a new language cannot create a fallback cycle)
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

            existing.IsMandatory = language.IsMandatory;

            // note that the service will prevent the default language from being "un-defaulted"
            // but does not hurt to test here - though the UI should prevent it too
            if (existing.IsDefault && !language.IsDefault)
            {
                ModelState.AddModelError("IsDefault", "Cannot un-default the default language.");
                throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
            }

            existing.IsDefault = language.IsDefault;
            existing.FallbackLanguageId = language.FallbackLanguageId;

            // modifying an existing language can create a fallback, verify
            // note that the service will check again, dealing with race conditions
            if (existing.FallbackLanguageId.HasValue)
            {
                var languages = Services.LocalizationService.GetAllLanguages().ToDictionary(x => x.Id, x => x);
                if (!languages.ContainsKey(existing.FallbackLanguageId.Value))
                {
                    ModelState.AddModelError("FallbackLanguage", "The selected fall back language does not exist.");
                    throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
                }
                if (CreatesCycle(existing, languages))
                {
                    ModelState.AddModelError("FallbackLanguage", $"The selected fall back language {languages[existing.FallbackLanguageId.Value].IsoCode} would create a circular path.");
                    throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
                }
            }

            Services.LocalizationService.Save(existing);
            return Mapper.Map<Language>(existing);
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
