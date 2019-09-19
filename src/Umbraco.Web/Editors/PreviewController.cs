using System;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Features;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    [DisableBrowserCache]
    public class PreviewController : Controller
    {
        [UmbracoAuthorize(redirectToUmbracoLogin: true)]
        public ActionResult Index()
        {
            var model = new BackOfficePreview
            {
                DisableDevicePreview = FeaturesResolver.Current.Features.Disabled.DisableDevicePreview,
                PreviewExtendedHeaderView = FeaturesResolver.Current.Features.Enabled.PreviewExtendedView
            };

            if (model.PreviewExtendedHeaderView.IsNullOrWhiteSpace() == false)
            {
                var viewEngineResult = ViewEngines.Engines.FindPartialView(ControllerContext, model.PreviewExtendedHeaderView);
                if (viewEngineResult.View == null)
                {
                    throw new InvalidOperationException("Could not find the view " + model.PreviewExtendedHeaderView + ", the following locations were searched: " + Environment.NewLine + string.Join(Environment.NewLine, viewEngineResult.SearchedLocations));
                }
            }

            return View(GlobalSettings.Path.EnsureEndsWith('/') + "Views/Preview/" + "Index.cshtml", model);
        }

        public ActionResult Editors(string editor)
        {
            if (string.IsNullOrEmpty(editor)) throw new ArgumentNullException("editor");
            return View(GlobalSettings.Path.EnsureEndsWith('/') + "Views/Preview/" + editor.Replace(".html", string.Empty) + ".cshtml");
        }
    }
}
