using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using umbraco.cms.businesslogic.member;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Controllers
{
    public class LoginController : SurfaceController
    {
        [HttpPost]
        public ActionResult HandleLogin([Bind(Prefix="loginModel")]LoginModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();    
            }

            //Validate credentials
            if (Membership.ValidateUser(model.Username, model.Password) == false)
            {
                ModelState.AddModelError("loginModel.Username", "Invalid username or password");
                return CurrentUmbracoPage();
            }
            //Set member online
            var member = Membership.GetUser(model.Username, true);
            if (member == null)
            {
                ModelState.AddModelError("Username", "Member not found");
                return CurrentUmbracoPage();
            }
            //Log them in
            FormsAuthentication.SetAuthCookie(member.UserName, true);

            return Redirect("/");            
        }
    }
}
