using System;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Features;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    [UmbracoAuthorize]
    [DisableBrowserCache]
    public class PreviewController : Controller
    {
        
        public ActionResult Index()
        {
            var model = new BackOfficePreview
            {
                DisableDevicePreview = FeaturesResolver.Current.Features.Disabled.DisableDevicePreview,
                PreviewExtendedView = FeaturesResolver.Current.Features.Enabled.PreviewExtendedView
            };

            if (model.PreviewExtendedView.IsNullOrWhiteSpace() == false)
            {
                var viewEngineResult = ViewEngines.Engines.FindPartialView(ControllerContext, model.PreviewExtendedView);
                if (viewEngineResult.View == null)
                {
                    throw new InvalidOperationException("Could not find the view " + model.PreviewExtendedView + ", the following locations were searched: " + Environment.NewLine + string.Join(Environment.NewLine, viewEngineResult.SearchedLocations));
                }
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
