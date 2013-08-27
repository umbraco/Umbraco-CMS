using System.Linq;
using System.Web.Mvc;
using umbraco.cms.businesslogic.member;
using Umbraco.Web.Mvc;
using Umbraco.Web.UmbracoModels;

namespace Umbraco.Web.UmbracoControllers
{
    public class LoginController : SurfaceController
    {
        [HttpPost]
        public ActionResult HandleLogin([Bind(Prefix="loginModel")]LoginModel model)
        {
            // TODO: Use new Member API
            if (ModelState.IsValid)
            {
                var m = Member.GetMemberFromLoginNameAndPassword(model.Username, model.Password);
                if (m != null)
                {
                    Member.AddMemberToCache(m);
                    return Redirect("/");
                }
            }

            return CurrentUmbracoPage();
        }
    }
}
