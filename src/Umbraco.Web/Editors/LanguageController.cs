using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Umbraco.Core.Persistence;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Backoffice controller supporting the dashboard for language administration.
    /// </summary>
    [PluginController("UmbracoApi")]
    public class LanguageController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Returns all cultures available for creating languages.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Culture> GetAllCultures()
        {
            return CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Select(x => new Culture {IsoCode = x.Name, Name = x.DisplayName});
        }

        /// <summary>
        /// Returns all currently configured languages.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Language> GetAllLanguages()
        {
            var allLanguages = Services.LocalizationService.GetAllLanguages();

            return allLanguages.Select(x => new Language
            {
                Id = x.Id,
                IsoCode = x.IsoCode,
                Name = x.CultureInfo.DisplayName,
                IsDefaultVariantLanguage = x.IsDefaultVariantLanguage,
                Mandatory = x.Mandatory
            });
        }
        
        /// <summary>
        /// Deletes a language with a given ID
        /// </summary>
        [HttpDelete]
        [HttpPost]
        public void DeleteLanguage(int id)
        {
            var language = Services.LocalizationService.GetLanguageById(id);
            if (language == null)
            {
                throw new EntityNotFoundException(id, $"Could not find language by id: '{id}'.");
            }

            if (language.IsDefaultVariantLanguage)
            {
                var message = $"Language with id '{id}' is currently set to 'default' and can not be deleted.";
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, message));
            }

            Services.LocalizationService.Delete(language);
        }

        /// <summary>
        /// Saves a bulk set of languages with default/mandatory settings and returns the full set of languages configured.
        /// </summary>
        [HttpPost]
        public IEnumerable<Language> SaveLanguages(IEnumerable<Language> languages)
        {
            foreach (var l in languages)
            {
                var language = Services.LocalizationService.GetLanguageByIsoCode(l.IsoCode);
                if (language == null)
                {
                    language = new Core.Models.Language(l.IsoCode);
                }
                language.Mandatory = l.Mandatory;
                language.IsDefaultVariantLanguage = l.IsDefaultVariantLanguage;
                Services.LocalizationService.Save(language);
            }

            return GetAllLanguages();
        }
    }
}