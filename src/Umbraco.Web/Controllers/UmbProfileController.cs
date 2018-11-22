using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;
using umbraco.cms.businesslogic.member;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Core.Security;
using Umbraco.Core;

namespace Umbraco.Web.Controllers
{
    [MemberAuthorize]
    public class UmbProfileController : SurfaceController
    {
        [HttpPost]
        public ActionResult HandleUpdateProfile([Bind(Prefix = "profileModel")] ProfileModel model)
        {
            var provider = global::Umbraco.Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
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
