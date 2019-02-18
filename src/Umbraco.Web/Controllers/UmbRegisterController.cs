using System;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Controllers
{
    public class UmbRegisterController : SurfaceController
    {
        public UmbRegisterController()
        {
        }

        public UmbRegisterController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, ILogger logger, IProfilingLogger profilingLogger, UmbracoHelper umbracoHelper)
            : base(umbracoContextAccessor, databaseFactory, services, appCaches, logger, profilingLogger, umbracoHelper)
        {
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleRegisterMember([Bind(Prefix = "registerModel")]RegisterModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            // U4-10762 Server error with "Register Member" snippet (Cannot save member with empty name)
            // If name field is empty, add the email address instead
            if (string.IsNullOrEmpty(model.Name) && string.IsNullOrEmpty(model.Email) == false)
            {
                model.Name = model.Email;
            }

            MembershipCreateStatus status;
            var member = Members.RegisterMember(model, out status, model.LoginOnSuccess);

            switch (status)
            {
                case MembershipCreateStatus.Success:

                    TempData["FormSuccess"] = true;

                    //if there is a specified path to redirect to then use it
                    if (model.RedirectUrl.IsNullOrWhiteSpace() == false)
                    {
                        return Redirect(model.RedirectUrl);
                    }
                    //redirect to current page by default

                    return RedirectToCurrentUmbracoPage();
                case MembershipCreateStatus.InvalidUserName:
                    ModelState.AddModelError((model.UsernameIsEmail || model.Username == null)
                        ? "registerModel.Email"
                        : "registerModel.Username",
                        "Username is not valid");
                    break;
                case MembershipCreateStatus.InvalidPassword:
                    ModelState.AddModelError("registerModel.Password", "The password is not strong enough");
                    break;
                case MembershipCreateStatus.InvalidQuestion:
                case MembershipCreateStatus.InvalidAnswer:
                    // TODO: Support q/a http://issues.umbraco.org/issue/U4-3213
                    throw new NotImplementedException(status.ToString());
                case MembershipCreateStatus.InvalidEmail:
                    ModelState.AddModelError("registerModel.Email", "Email is invalid");
                    break;
                case MembershipCreateStatus.DuplicateUserName:
                    ModelState.AddModelError((model.UsernameIsEmail || model.Username == null)
                        ? "registerModel.Email"
                        : "registerModel.Username",
                        "A member with this username already exists.");
                    break;
                case MembershipCreateStatus.DuplicateEmail:
                    ModelState.AddModelError("registerModel.Email", "A member with this e-mail address already exists");
                    break;
                case MembershipCreateStatus.UserRejected:
                case MembershipCreateStatus.InvalidProviderUserKey:
                case MembershipCreateStatus.DuplicateProviderUserKey:
                case MembershipCreateStatus.ProviderError:
                    //don't add a field level error, just model level
                    ModelState.AddModelError("registerModel", "An error occurred creating the member: " + status);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return CurrentUmbracoPage();
        }

    }
}
