using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.WebAssets;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.BackOffice.ActionResults;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Editors;
using Umbraco.Web.Features;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Security;
using Umbraco.Web.Services;
using Umbraco.Web.Trees;
using Umbraco.Web.WebAssets;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.BackOffice.Controllers
{
    [DisableBrowserCache]
    [Area(Constants.Web.Mvc.BackOfficeArea)]
    public class PreviewController : Controller
    {
        private readonly UmbracoFeatures _features;
        private readonly GlobalSettings _globalSettings;
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly IBackofficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ICookieManager _cookieManager;
        private readonly IRuntimeMinifier _runtimeMinifier;
        private readonly ICompositeViewEngine _viewEngines;

        public PreviewController(
            UmbracoFeatures features,
            IOptions<GlobalSettings> globalSettings,
            IPublishedSnapshotService publishedSnapshotService,
            IBackofficeSecurityAccessor backofficeSecurityAccessor,
            ILocalizationService localizationService,
            IHostingEnvironment hostingEnvironment,
            ICookieManager cookieManager,
            IRuntimeMinifier runtimeMinifier,
            ICompositeViewEngine viewEngines)
        {
            _features = features;
            _globalSettings = globalSettings.Value;
            _publishedSnapshotService = publishedSnapshotService;
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
            _localizationService = localizationService;
            _hostingEnvironment = hostingEnvironment;
            _cookieManager = cookieManager;
            _runtimeMinifier = runtimeMinifier;
            _viewEngines = viewEngines;
        }

        [UmbracoAuthorize(redirectToUmbracoLogin: true, requireApproval : false)]
        [DisableBrowserCache]
        public ActionResult Index()
        {
            var availableLanguages = _localizationService.GetAllLanguages();

            var model = new BackOfficePreviewModel(_features, availableLanguages);

            if (model.PreviewExtendedHeaderView.IsNullOrWhiteSpace() == false)
            {
                var viewEngineResult = _viewEngines.FindView(ControllerContext, model.PreviewExtendedHeaderView, false);
                if (viewEngineResult.View == null)
                    throw new InvalidOperationException("Could not find the view " + model.PreviewExtendedHeaderView + ", the following locations were searched: " + Environment.NewLine + string.Join(Environment.NewLine, viewEngineResult.SearchedLocations));
            }

            var viewPath = Path.Combine(
                _globalSettings.UmbracoPath,
                Constants.Web.Mvc.BackOfficeArea,
                ControllerExtensions.GetControllerName<PreviewController>() + ".cshtml")
                .Replace("\\", "/"); // convert to forward slashes since it's a virtual path

            return View(viewPath, model);
        }

        /// <summary>
        /// Returns the JavaScript file for preview
        /// </summary>
        /// <returns></returns>
        [MinifyJavaScriptResult(Order = 0)]
        // TODO: Replace this with response caching https://docs.microsoft.com/en-us/aspnet/core/performance/caching/response?view=aspnetcore-3.1
        //[OutputCache(Order = 1, VaryByParam = "none", Location = OutputCacheLocation.Server, Duration = 5000)]
        public async Task<JavaScriptResult> Application()
        {
            var files = await _runtimeMinifier.GetAssetPathsAsync(BackOfficeWebAssets.UmbracoPreviewJsBundleName);
            var result = BackOfficeJavaScriptInitializer.GetJavascriptInitialization(files, "umbraco.preview", _globalSettings, _hostingEnvironment);

            return new JavaScriptResult(result);
        }

        /// <summary>
        /// The endpoint that is loaded within the preview iframe
        /// </summary>
        /// <returns></returns>
        [UmbracoAuthorize]
        public ActionResult Frame(int id, string culture)
        {
            var user = _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser;

            var previewToken = _publishedSnapshotService.EnterPreview(user, id);

            _cookieManager.SetCookieValue(Constants.Web.PreviewCookieName, previewToken);

            // use a numeric url because content may not be in cache and so .Url would fail
            var query = culture.IsNullOrWhiteSpace() ? string.Empty : $"?culture={culture}";

            return RedirectPermanent($"../../{id}.aspx{query}");
        }

        public ActionResult End(string redir = null)
        {
            var previewToken = _cookieManager.GetPreviewCookieValue();

            _publishedSnapshotService.ExitPreview(previewToken);

            _cookieManager.ExpireCookie(Constants.Web.PreviewCookieName);

            if (Uri.IsWellFormedUriString(redir, UriKind.Relative)
                && redir.StartsWith("//") == false
                && Uri.TryCreate(redir, UriKind.Relative, out var url))
                return Redirect(url.ToString());

            return Redirect("/");
        }
    }
}
