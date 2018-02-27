using System;
using System.Web.Mvc;
using Umbraco.Web.Features;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    [UmbracoAuthorize]
    [DisableBrowserCache]
    public class PreviewController : Controller
    {
        private const string ViewsPath = "~/Umbraco/Views/Preview/";

        public ActionResult Index()
        {
            ViewData["DisableDevicePreview"] = FeaturesResolver.Current.Features.Disabled.DevicePreview;
            return View(ViewsPath + "Index.cshtml");
        }

        [AllowAnonymous]
        public ActionResult Editors(string editor)
        {
            if (string.IsNullOrEmpty(editor)) throw new ArgumentNullException("editor");
            return View(ViewsPath + editor.Replace(".html", string.Empty) + ".cshtml");
        }
    }
}
