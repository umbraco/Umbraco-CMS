using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using umbraco.cms.businesslogic.member;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Controllers
{
    public class UmbLoginStatusController : SurfaceController
    {
        [HttpPost]
        public ActionResult HandleLogout([Bind(Prefix = "loginStatusModel")]LoginStatusModel model)
        {
            if (ModelState.IsValid == false)
            {
                return CurrentUmbracoPage();
            }

            if (HttpContext.User != null && HttpContext.User.Identity.IsAuthenticated)
            {
                FormsAuthentication.SignOut();
            }

            //TODO: Shouldn't we be redirecting to the current page or integrating this with the 
            // normal Umbraco protection stuff?
            return Redirect("/");
        }
    }
}
