using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Features;
using Umbraco.Web.JavaScript;
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
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ILocalizationService _localizationService;

        public PreviewController(
            UmbracoFeatures features,
            IGlobalSettings globalSettings,
            IPublishedSnapshotService publishedSnapshotService,
            IUmbracoContextAccessor umbracoContextAccessor,
            ILocalizationService localizationService)
        {
            _features = features;
            _globalSettings = globalSettings;
            _publishedSnapshotService = publishedSnapshotService;
            _umbracoContextAccessor = umbracoContextAccessor;
            _localizationService = localizationService;
        }

        [UmbracoAuthorize(redirectToUmbracoLogin: true)]
        [DisableBrowserCache]
        public ActionResult Index()
        {
            var availableLanguages = _localizationService.GetAllLanguages();

            var model = new BackOfficePreviewModel(_features, _globalSettings, availableLanguages);

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
        /// Returns the JavaScript file for preview
        /// </summary>
        /// <returns></returns>
        [MinifyJavaScriptResult(Order = 0)]
        [OutputCache(Order = 1, VaryByParam = "none", Location = OutputCacheLocation.Server, Duration = 5000)]
        public JavaScriptResult Application()
        {
            var files = JsInitialization.OptimizeScriptFiles(HttpContext, JsInitialization.GetPreviewInitialization());
            var result = JsInitialization.GetJavascriptInitialization(HttpContext, files, "umbraco.preview");

            return JavaScript(result);
        }

        /// <summary>
        /// The endpoint that is loaded within the preview iframe
        /// </summary>
        /// <returns></returns>
        [UmbracoAuthorize]
        public ActionResult Frame(int id, string culture)
        {
            var user = _umbracoContextAccessor.UmbracoContext.Security.CurrentUser;

            var previewToken = _publishedSnapshotService.EnterPreview(user, id);

            Response.Cookies.Set(new HttpCookie(Constants.Web.PreviewCookieName, previewToken));

            // use a numeric url because content may not be in cache and so .Url would fail
            var query = culture.IsNullOrWhiteSpace() ? string.Empty : $"?culture={culture}";
            Response.Redirect($"../../{id}.aspx{query}", true);

            return null;
        }

        public ActionResult End(string redir = null)
        {
            var previewToken = Request.GetPreviewCookieValue();
            var service = Current.PublishedSnapshotService;
            service.ExitPreview(previewToken);

            System.Web.HttpContext.Current.ExpireCookie(Constants.Web.PreviewCookieName);

            if (Uri.IsWellFormedUriString(redir, UriKind.Relative)
                && redir.StartsWith("//") == false
                && Uri.TryCreate(redir, UriKind.Relative, out Uri url))
            {
                return Redirect(url.ToString());
            }

            return Redirect("/");
        }
    }
}
