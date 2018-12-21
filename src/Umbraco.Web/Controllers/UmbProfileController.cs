using System;
using System.Web.Mvc;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Core.Security;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;

namespace Umbraco.Web.Controllers
{
    [MemberAuthorize]
    public class UmbProfileController : SurfaceController
    {
        // fixme - delete?
        public UmbProfileController()
        {
        }

        public UmbProfileController(UmbracoContext umbracoContext, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, CacheHelper applicationCache, ILogger logger, IProfilingLogger profilingLogger) : base(umbracoContext, databaseFactory, services, applicationCache, logger, profilingLogger)
        {
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleUpdateProfile([Bind(Prefix = "profileModel")] ProfileModel model)
        {
            var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
            if (provider.IsUmbracoMembershipProvider() == false)
            {
                throw new NotSupportedException("Profile editing with the " + typeof(UmbProfileController) + " is not supported when not using the default Umbraco membership provider");
            }

            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            var updateAttempt = Members.UpdateMemberProfile(model);
            if (updateAttempt.Success == false)
            {
                //don't add a field level error, just model level
                ModelState.AddModelError("profileModel", updateAttempt.Exception.Message);
                return CurrentUmbracoPage();
            }

            TempData["ProfileUpdateSuccess"] = true;

            //if there is a specified path to redirect to then use it
            if (model.RedirectUrl.IsNullOrWhiteSpace() == false)
            {
                return Redirect(model.RedirectUrl);
            }

            //redirect to current page by default
            return RedirectToCurrentUmbracoPage();
        }
    }
}
