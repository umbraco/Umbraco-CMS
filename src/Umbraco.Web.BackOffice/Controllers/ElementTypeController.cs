// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

[PluginController("UmbracoApi")]
public class ElementTypeController : UmbracoAuthorizedJsonController
{
    private readonly IContentTypeService _contentTypeService;

    public ElementTypeController(IContentTypeService contentTypeService) => _contentTypeService = contentTypeService;

    [HttpGet]
    public IEnumerable<object> GetAll() =>
        _contentTypeService
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
