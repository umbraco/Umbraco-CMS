using System;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Composing;
using Umbraco.Web.Features;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{
    [DisableBrowserCache]
    public class PreviewController : Controller
    {
        private readonly UmbracoFeatures _features;
        private readonly IGlobalSettings _globalSettings;
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly UmbracoContext _umbracoContext;

        public PreviewController(UmbracoFeatures features, IGlobalSettings globalSettings, IPublishedSnapshotService publishedSnapshotService, UmbracoContext umbracoContext)
        {
            _features = features;
            _globalSettings = globalSettings;
            _publishedSnapshotService = publishedSnapshotService;
            _umbracoContext = umbracoContext;
        }

        [UmbracoAuthorize(redirectToUmbracoLogin: true)]
        public ActionResult Index()
        {
            var model = new BackOfficePreview
            {
                DisableDevicePreview = _features.Disabled.DisableDevicePreview,
                PreviewExtendedHeaderView = _features.Enabled.PreviewExtendedView
            };

            if (model.PreviewExtendedHeaderView.IsNullOrWhiteSpace() == false)
            {
                var viewEngineResult = ViewEngines.Engines.FindPartialView(ControllerContext, model.PreviewExtendedHeaderView);
                if (viewEngineResult.View == null)
                {
                    throw new InvalidOperationException("Could not find the view " + model.PreviewExtendedHeaderView + ", the following locations were searched: " + Environment.NewLine + string.Join(Environment.NewLine, viewEngineResult.SearchedLocations));
                }
            }

            return View(_globalSettings.Path.EnsureEndsWith('/') + "Views/Preview/" + "Index.cshtml", model);
        }

        /// <summary>
        /// The endpoint that is loaded within the preview iframe
        /// </summary>
        /// <returns></returns>
        [UmbracoAuthorize]
        public ActionResult Frame(int id)
        {
            var user = _umbracoContext.Security.CurrentUser;

            var previewToken = _publishedSnapshotService.EnterPreview(user, id);

            Response.Cookies.Set(new HttpCookie(Constants.Web.PreviewCookieName, previewToken));

            // use a numeric url because content may not be in cache and so .Url would fail
            Response.Redirect($"../../{id}.aspx", true);

            return null;
        }

        //fixme: not sure we need this anymore since there is no canvas editing - then we can remove that route too
        public ActionResult Editors(string editor)
        {
            if (string.IsNullOrEmpty(editor)) throw new ArgumentNullException(nameof(editor));
            return View(_globalSettings.Path.EnsureEndsWith('/') + "Views/Preview/" + editor.Replace(".html", string.Empty) + ".cshtml");
        }
    }
}
