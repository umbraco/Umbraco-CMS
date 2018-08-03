using AutoMapper;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using System.Linq;

using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    /// <inheritdoc />
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.Languages)]
    [EnableOverrideAuthorization]
    public class LocalizationController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LanguageForContentTranslation> GetAllLanguages()
        {
            return Mapper.Map<IEnumerable<ILanguage>, IEnumerable<LanguageForContentTranslation>>(Services.LocalizationService.GetAllLanguages());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public LanguageForContentTranslation GetNodeCulture(int nodeId)
        {
            var domain = Services.DomainService.GetAssignedDomains(nodeId, true).FirstOrDefault();

            if (domain != null)
            {
                var language = Services.LocalizationService.GetLanguageByIsoCode(domain.LanguageIsoCode);

                if (language != null)
                {
                    return Mapper.Map<ILanguage, LanguageForContentTranslation>(language);
                }
            }

            return null;
        }
    }
}
