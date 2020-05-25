using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core.Services;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Editors;

namespace Umbraco.Web.BackOffice.PropertyEditors
{
    [PluginController("UmbracoApi")]
    public class NestedContentController : UmbracoAuthorizedJsonController
    {
        private readonly IContentTypeService _contentTypeService;

        public NestedContentController(IContentTypeService contentTypeService)
        {
            _contentTypeService = contentTypeService;
        }

        [HttpGet]
        public IEnumerable<object> GetContentTypes()
        {
            return _contentTypeService
                .GetAllElementTypes()
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
