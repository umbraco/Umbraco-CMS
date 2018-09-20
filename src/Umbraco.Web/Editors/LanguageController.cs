﻿using System.Collections.Generic;
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
            if (language == null) return NotFound();

            // the service would not let us do it, but test here nevertheless
            if (language.IsDefaultVariantLanguage)
            {
                var message = $"Language '{language.IsoCode}' is currently set to 'default' and can not be deleted.";
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

            // this is prone to race conds but the service will not let us proceed anyways
            var existing = Services.LocalizationService.GetLanguageByIsoCode(language.IsoCode);

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

                //create it
                var newLang = new Umbraco.Core.Models.Language(culture.Name)
                {
                    CultureName = culture.DisplayName,
                    IsDefaultVariantLanguage = language.IsDefaultVariantLanguage,
                    Mandatory = language.Mandatory
                };
                Services.LocalizationService.Save(newLang);
                return Mapper.Map<Language>(newLang);
            }

            existing.Mandatory = language.Mandatory;

            // note that the service will prevent the default language from being "un-defaulted"
            // but does not hurt to test here - though the UI should prevent it too
            if (existing.IsDefaultVariantLanguage && !language.IsDefaultVariantLanguage)
            {
                ModelState.AddModelError("IsDefaultVariantLanguage", "Cannot un-default the default language.");
                throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
            }

            existing.IsDefaultVariantLanguage = language.IsDefaultVariantLanguage;

            Services.LocalizationService.Save(existing);
            return Mapper.Map<Language>(existing);
        }        
    }
}
