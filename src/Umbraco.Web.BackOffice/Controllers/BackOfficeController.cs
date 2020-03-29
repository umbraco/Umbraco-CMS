using Microsoft.AspNetCore.Mvc;
using Umbraco.Composing;

namespace Umbraco.Web.BackOffice.Controllers
{
    public class BackOfficeController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View();
        }
    }
}
