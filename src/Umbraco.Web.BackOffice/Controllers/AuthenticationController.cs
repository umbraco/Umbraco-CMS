using Microsoft.AspNetCore.Mvc;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]  // TODO: Maybe this could be applied with our Application Model conventions
    //[ValidationFilter] // TODO: I don't actually think this is required with our custom Application Model conventions applied
    [TypeFilter(typeof(AngularJsonOnlyConfigurationAttribute))] // TODO: This could be applied with our Application Model conventions
    [IsBackOffice] // TODO: This could be applied with our Application Model conventions
    public class AuthenticationController : ControllerBase
    {
        // TODO: We need to import the logic from Umbraco.Web.Editors.AuthenticationController and it should not be an auto-routed api controller
    }
}
