using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AutoMapper;
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
    public class LanguageController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Returns all cultures available for creating languages.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Culture> GetAllCultures()
        {
            return Mapper.Map<IEnumerable<Culture>>(CultureInfo.GetCultures(CultureTypes.AllCultures));
        }

        /// <summary>
        /// Returns all currently configured languages.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<LanguageDisplay> GetAllLanguages()
        {
            var allLanguages = Services.LocalizationService.GetAllLanguages();
            return Mapper.Map<IEnumerable<LanguageDisplay>>(allLanguages);
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
        /// Saves a bulk set of languages with default/mandatory settings and returns the full set of languages configured.
        /// </summary>
        [HttpPost]
        public IEnumerable<LanguageDisplay> SaveLanguages(IEnumerable<LanguageDisplay> languages)
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
