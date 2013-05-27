using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Umbraco.Core.Configuration;
using Umbraco.Core;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// A controller to render out the default back office view
    /// </summary>
    public class BackOfficeController : Controller
    {

        /// <summary>
        /// Render the default view
        /// </summary>
        /// <returns></returns>
        public ActionResult Default()
        {
            return View(GlobalSettings.Path.EnsureEndsWith('/') + "Views/Default.cshtml");
        }

    }
}
