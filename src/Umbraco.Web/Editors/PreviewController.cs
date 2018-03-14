using System;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Features;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    [UmbracoAuthorize]
    [DisableBrowserCache]
    public class PreviewController : Controller
    {
        
        public ActionResult Index()
        {
            ViewData["DisableDevicePreview"] = FeaturesResolver.Current.Features.Disabled.DevicePreview;
            if (string.IsNullOrWhiteSpace(FeaturesResolver.Current.Features.Enabled.ExtendPreviewHtml) == false)
            {
                ViewData["ExtendPreviewHtml"] = FeaturesResolver.Current.Features.Enabled.ExtendPreviewHtml;
            }
            return View(GlobalSettings.Path.EnsureEndsWith('/') + "Views/Preview/" + "Index.cshtml");
        }

        [AllowAnonymous]
        public ActionResult Editors(string editor)
        {
            if (string.IsNullOrEmpty(editor)) throw new ArgumentNullException("editor");
            return View(GlobalSettings.Path.EnsureEndsWith('/') + "Views/Preview/" + editor.Replace(".html", string.Empty) + ".cshtml");
        }
    }
}
