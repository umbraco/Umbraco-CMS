using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
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
            return Services.ContentTypeService
                .GetAllElementTypes()
                .OrderBy(x => x.SortOrder)
                .Select(x => new
                {
                    id = x.Id,
                    guid = x.Key,
                    name = x.Name,
                    alias = x.Alias,
                    icon = x.Icon,
                    tabs = x.CompositionPropertyGroups.Where(x => x.Type == PropertyGroupType.Group && x.GetParentAlias() == null).Select(y => y.Name).Distinct()
                });
        }
    }
}
