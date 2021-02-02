using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Umbraco.Core.Services;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    public class ElementTypeController : UmbracoAuthorizedJsonController
    {
        [HttpGet]
        public IEnumerable<object> GetAll()
        {
            return Services.ContentTypeService
                .GetAllElementTypes()
                .OrderBy(x => x.SortOrder)
                .Select(x => new
                {
                    id = x.Id,
                    key = x.Key,
                    name = x.Name,
                    description = x.Description,
                    alias = x.Alias,
                    icon = x.Icon
                });
        }
    }
}
