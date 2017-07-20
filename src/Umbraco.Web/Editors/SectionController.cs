using System.Collections.Generic;
using AutoMapper;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using System.Linq;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for using the list of sections
    /// </summary>
    [PluginController("UmbracoApi")]
    public class SectionController : UmbracoAuthorizedJsonController
    {
        public IEnumerable<Section> GetSections()
        {
            var sections =  Services.SectionService.GetAllowedSections(Security.GetUserId());
            return sections.Select(Mapper.Map<Core.Models.Section, Section>);
        }
    }
}
