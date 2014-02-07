using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
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
            var sections =  Services.SectionService.GetAllowedSections(UmbracoUser.Id);
            return sections.Select(Mapper.Map<Core.Models.Section, Section>);
        } 
    }
}