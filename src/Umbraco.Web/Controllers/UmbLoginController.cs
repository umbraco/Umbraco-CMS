using System.Linq;
using System.Web.Mvc;
using umbraco.cms.businesslogic.member;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.Controllers
{
    public class UmbLoginController : SurfaceController
    {
        [HttpPost]
        public ActionResult HandleLogin([Bind(Prefix="loginModel")]LoginModel model)
        {
            var doc = new Document(1089);
            var prop = doc.getProperty("cardinal");

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
