// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.PropertyEditors;

[PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
public class NestedContentController : UmbracoAuthorizedJsonController
{
    private readonly IContentTypeService _contentTypeService;

    public NestedContentController(IContentTypeService contentTypeService) => _contentTypeService = contentTypeService;

    [HttpGet]
    public IEnumerable<object> GetContentTypes() => _contentTypeService
        .GetAllElementTypes()
        .OrderBy(x => x.SortOrder)
        .Select(x => new
        {
            id = x.Id,
            guid = x.Key,
            name = x.Name,
            alias = x.Alias,
            icon = x.Icon,
            tabs = x.CompositionPropertyGroups
                .Where(x => x.Type == PropertyGroupType.Group && x.GetParentAlias() is null)
                .Select(y => y.Name).Distinct()
        });
}
