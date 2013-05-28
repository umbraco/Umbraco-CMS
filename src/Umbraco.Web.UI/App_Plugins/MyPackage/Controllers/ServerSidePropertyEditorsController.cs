using System.Web.Mvc;

namespace Umbraco.Belle.App_Plugins.MyPackage.Controllers
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
