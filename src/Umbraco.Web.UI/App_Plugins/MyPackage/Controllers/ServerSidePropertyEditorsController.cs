using System.Web.Mvc;

namespace Umbraco.Web.UI.App_Plugins.MyPackage.Controllers
{

    public class ServerSidePropertyEditorsController : Controller
    {
        [HttpGet]
        public ActionResult ServerEnvironment()
        {
            return View();
        }

    }
}
