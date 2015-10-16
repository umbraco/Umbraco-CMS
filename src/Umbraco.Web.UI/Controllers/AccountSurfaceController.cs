namespace Umbraco.Web.UI.Controllers
{
    using System.Web.Mvc;
    using System.Web.Security;
    using Mvc;

    public class AccountSurfaceController : SurfaceController
    {
        public ContentResult TestLogin(string username, string password)
        {
            return Content(Membership.ValidateUser(username, password).ToString());
        }
   }
}