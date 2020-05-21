using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Security;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]  // TODO: Maybe this could be applied with our Application Model conventions
    //[ValidationFilter] // TODO: I don't actually think this is required with our custom Application Model conventions applied
    [TypeFilter(typeof(AngularJsonOnlyConfigurationAttribute))] // TODO: This could be applied with our Application Model conventions
    [IsBackOffice] // TODO: This could be applied with our Application Model conventions
    public class AuthenticationController : UmbracoApiController
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        // TODO: We need to import the logic from Umbraco.Web.Editors.AuthenticationController and it should not be an auto-routed api controller

        public AuthenticationController(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Checks if the current user's cookie is valid and if so returns OK or a 400 (BadRequest)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public bool IsAuthenticated()
        {
            var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
            var attempt = umbracoContext.Security.AuthorizeRequest();
            if (attempt == ValidateRequestAttempt.Success)
            {
                return true;
            }
            return false;
        }
    }
}
