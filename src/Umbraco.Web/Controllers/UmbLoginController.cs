using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using umbraco.cms.businesslogic.member;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Controllers
{
    public class UmbLoginController : SurfaceController
    {
        [HttpPost]
        public ActionResult HandleLogin([Bind(Prefix="loginModel")]LoginModel model)
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

            return Redirect("/");            
        }
    }
}
