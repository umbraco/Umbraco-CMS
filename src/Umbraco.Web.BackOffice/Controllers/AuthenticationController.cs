using Microsoft.AspNetCore.Mvc;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Filters;

namespace Umbraco.Web.BackOffice.Controllers
{
    [Area(Umbraco.Core.Constants.Web.Mvc.BackOfficeArea)]
    //[ValidationFilter] // TODO: I don't actually think this is required with our custom Application Model conventions applied
    [TypeFilter(typeof(AngularJsonOnlyConfigurationAttribute))]
    [IsBackOffice]
    public class AuthenticationController : ControllerBase
    {
        // TODO: We need to import the logic from Umbraco.Web.Editors.AuthenticationController and it should not be an auto-routed api controller
    }
}
