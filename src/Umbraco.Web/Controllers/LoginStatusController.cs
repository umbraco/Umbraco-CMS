using System.Linq;
using System.Web.Mvc;
using umbraco.cms.businesslogic.member;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Controllers
{
    public class LoginStatusController : SurfaceController
    {
        [HttpPost]
        public ActionResult HandleLogout([Bind(Prefix = "loginStatusModel")]LoginStatusModel model)
        {
            // TODO: Use new Member API
            if (ModelState.IsValid)
            {
                if (Member.IsLoggedOn())
                {
                    var memberId = Member.CurrentMemberId();
                    Member.RemoveMemberFromCache(memberId);
                    Member.ClearMemberFromClient(memberId);
                }

                return Redirect("/");                
            }

            return CurrentUmbracoPage();
        }
    }
}
