using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

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
            var allLanguages = Services.LocalizationService.GetAllLanguages().OrderBy(x => x.Id).ToList();
            var langs = Mapper.Map<IEnumerable<Language>>(allLanguages).ToList();

            //if there's only one language, by default it is the default
            if (langs.Count == 1)
            {
                langs[0].IsDefaultVariantLanguage = true;
                langs[0].Mandatory = true;
            }   
            else if (allLanguages.All(x => !x.IsDefaultVariantLanguage))
            {
                //if no language has the default flag, then the defaul language is the one with the lowest id
                langs[0].IsDefaultVariantLanguage = true;
                langs[0].Mandatory = true;
            }

            return langs.OrderBy(x => x.Name);
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
            if (!lang.IsDefaultVariantLanguage)
            {
                if (allLangs.Count == 1)
                {
                    model.IsDefaultVariantLanguage = true;
                    model.Mandatory = true;
                }   
                else if (allLangs.All(x => !x.IsDefaultVariantLanguage))
                {
                    //if no language has the default flag, then the defaul language is the one with the lowest id
                    model.IsDefaultVariantLanguage = allLangs[0].Id == lang.Id;
                    model.Mandatory = allLangs[0].Id == lang.Id;
                }
            }
            

            return model;
        }

        /// <summary>
        /// Deletes a language with a given ID
        /// </summary>
        [HttpDelete]
        [HttpPost]
        public IHttpActionResult DeleteLanguage(int id)
        {
            var language = Services.LocalizationService.GetLanguageById(id);
            if (language == null) return NotFound();

            var totalLangs = Services.LocalizationService.GetAllLanguages().Count();

            if (language.IsDefaultVariantLanguage || totalLangs == 1)
            {
                var message = $"Language '{language.IsoCode}' is currently set to 'default' or it is the only installed language and can not be deleted.";
                throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse(message));
            }

            Services.LocalizationService.Delete(language);

            return Ok();
        }

        /// <summary>
        /// Creates or saves a language
        /// </summary>
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
                var newLang = new Umbraco.Core.Models.Language(culture.Name)
                {
                    CultureName = culture.DisplayName,
                    IsDefaultVariantLanguage = language.IsDefaultVariantLanguage,
                    Mandatory = language.Mandatory
                };
                Services.LocalizationService.Save(newLang);
                return Mapper.Map<Language>(newLang);
            }

            found.Mandatory = language.Mandatory;
            found.IsDefaultVariantLanguage = language.IsDefaultVariantLanguage;
            Services.LocalizationService.Save(found);
            return Mapper.Map<Language>(found);
        }
        
    }
}
