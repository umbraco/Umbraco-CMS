using System.Collections.Generic;
using AutoMapper;
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
    public class SectionController : UmbracoAuthorizedApiController
    {
        public IEnumerable<Section> GetSections()
        {
            return Core.Sections.SectionCollection.Sections.Select(Mapper.Map<Core.Sections.Section, Section>);
        } 
    }
}