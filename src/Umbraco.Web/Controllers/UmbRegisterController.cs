using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Controllers
{
    public class UmbRegisterController : SurfaceController
    {
        [HttpPost]
        public ActionResult HandleRegisterMember([Bind(Prefix = "registerModel")]RegisterModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
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
                    //TODO: Support q/a http://issues.umbraco.org/issue/U4-3213
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
