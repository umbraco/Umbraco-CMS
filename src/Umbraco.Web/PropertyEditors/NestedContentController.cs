using System.Collections.Generic;
using System.Linq;
using Umbraco.Web.Editors;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.PropertyEditors
{
    [PluginController("UmbracoApi")]
    public class NestedContentController : UmbracoAuthorizedJsonController
    {
        [System.Web.Http.HttpGet]
        public IEnumerable<object> GetContentTypes()
        {
            return Services.ContentTypeService.GetAllContentTypes()
                .OrderBy(x => x.SortOrder)
                .Select(x => new
                {
                    id = x.Id,
                    guid = x.Key,
                    name = x.Name,
                    alias = x.Alias,
                    icon = x.Icon,
                    tabs = x.CompositionPropertyGroups.Select(y => y.Name).Distinct()
                });
        }
    }
}
