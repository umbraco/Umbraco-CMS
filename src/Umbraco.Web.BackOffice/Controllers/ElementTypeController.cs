// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using Umbraco.Web.Common.Attributes;

namespace Umbraco.Web.BackOffice.Controllers
{
    [PluginController("UmbracoApi")]
    public class ElementTypeController : UmbracoAuthorizedJsonController
    {
        private readonly IContentTypeService _contentTypeService;

        public ElementTypeController(IContentTypeService contentTypeService)
        {
            _contentTypeService = contentTypeService;
        }

        [HttpGet]
        public IEnumerable<object> GetAll()
        {
            return _contentTypeService
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
