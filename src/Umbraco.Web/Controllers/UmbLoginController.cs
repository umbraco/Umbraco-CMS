using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using umbraco.cms.businesslogic.member;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Core;

namespace Umbraco.Web.Controllers
{
    public class UmbLoginController : SurfaceController
    {
        [HttpPost]
        public ActionResult HandleLogin([Bind(Prefix = "loginModel")]LoginModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            if (Members.Login(model.Username, model.Password) == false)
            {
                //don't add a field level error, just model level
                ModelState.AddModelError("loginModel", "Invalid username or password");
                return CurrentUmbracoPage();
            }

            TempData["LoginSuccess"] = true;

            //if there is a specified path to redirect to then use it
            if (model.RedirectUrl.IsNullOrWhiteSpace() == false)
            {
                // validate the redirect url
                if (Url.IsLocalUrl(model.RedirectUrl))
                {
                    return Redirect(model.RedirectUrl);
                }
                else
                {
                    // if it's not a local url we'll redirect to the root of the current site
                    return Redirect(base.CurrentPage.Site().Url);
                }
            }

            //redirect to current page by default

            return RedirectToCurrentUmbracoPage();
        }
    }
}
