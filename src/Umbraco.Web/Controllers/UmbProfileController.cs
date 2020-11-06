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
using Umbraco.Web.Security;

namespace Umbraco.Web.Controllers
{
    [MemberAuthorize]
    public class UmbProfileController : SurfaceController
    {
        private readonly MembershipHelper _membershipHelper;

        public UmbProfileController()
        { }

        public UmbProfileController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory,
            ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger,
            MembershipHelper membershipHelper)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger)
        {
            _membershipHelper = membershipHelper;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateUmbracoFormRouteString]
        public ActionResult HandleUpdateProfile([Bind(Prefix = "profileModel")] ProfileModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            var updateAttempt = _membershipHelper.UpdateMemberProfile(model);
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
