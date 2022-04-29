using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

[PluginController(Constants.Web.Mvc.BackOfficeArea)]
[Authorize(Policy = AuthorizationPolicies.TreeAccessDocumentTypes)]
public class PropertyTypeController : UmbracoAuthorizedJsonController
{
    private readonly IPropertyTypeUsageService _propertyTypeUsageService;

    public PropertyTypeController(IPropertyTypeUsageService propertyTypeUsageService) => _propertyTypeUsageService = propertyTypeUsageService;

    public ActionResult<PropertyTypeHasValuesDisplay> HasValues(string alias)
    {
        bool hasValue = _propertyTypeUsageService.HasSavedPropertyValues(alias);
        return new PropertyTypeHasValuesDisplay(alias, hasValue);
    }
}
