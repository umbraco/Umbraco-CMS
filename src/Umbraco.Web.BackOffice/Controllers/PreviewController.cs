using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Core.Services;
using Umbraco.Core.WebAssets;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.ActionResults;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Editors;
using Umbraco.Web.Features;
using Umbraco.Web.PublishedCache;
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
        private readonly IGlobalSettings _globalSettings;
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IContentSettings _contentSettings;
        private readonly TreeCollection _treeCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ICookieManager _cookieManager;
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly ISecuritySettings _securitySettings;
        private readonly IRuntimeMinifier _runtimeMinifier;
        private readonly ICompositeViewEngine _viewEngines;

        public PreviewController(
            UmbracoFeatures features,
            IGlobalSettings globalSettings,
            IPublishedSnapshotService publishedSnapshotService,
            IUmbracoContextAccessor umbracoContextAccessor,
            ILocalizationService localizationService,
            IUmbracoVersion umbracoVersion,
            IContentSettings contentSettings,
            TreeCollection treeCollection,
            IHttpContextAccessor httpContextAccessor,
            IHostingEnvironment hostingEnvironment,
            ICookieManager cookieManager,
            IRuntimeSettings settings,
            ISecuritySettings securitySettings,
            IRuntimeMinifier runtimeMinifier,
            ICompositeViewEngine viewEngines)
        {
            _features = features;
            _globalSettings = globalSettings;
            _publishedSnapshotService = publishedSnapshotService;
            _umbracoContextAccessor = umbracoContextAccessor;
            _localizationService = localizationService;
            _umbracoVersion = umbracoVersion;
            _contentSettings = contentSettings ?? throw new ArgumentNullException(nameof(contentSettings));
            _treeCollection = treeCollection;
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
            _cookieManager = cookieManager;
            _runtimeSettings = settings;
            _securitySettings = securitySettings;
            _runtimeMinifier = runtimeMinifier;
            _viewEngines = viewEngines;
        }

        [UmbracoAuthorize(redirectToUmbracoLogin: true)]
        [DisableBrowserCache]
        public ActionResult Index()
        {
            var availableLanguages = _localizationService.GetAllLanguages();

            var model = new BackOfficePreviewModel(_features, _globalSettings, _umbracoVersion, availableLanguages, _contentSettings, _treeCollection, _hostingEnvironment, _runtimeSettings, _securitySettings);

            if (model.PreviewExtendedHeaderView.IsNullOrWhiteSpace() == false)
            {
                var viewEngineResult = _viewEngines.FindView(ControllerContext, model.PreviewExtendedHeaderView, false);
                if (viewEngineResult.View == null)
                    throw new InvalidOperationException("Could not find the view " + model.PreviewExtendedHeaderView + ", the following locations were searched: " + Environment.NewLine + string.Join(Environment.NewLine, viewEngineResult.SearchedLocations));
            }

            return View(_globalSettings.GetBackOfficePath(_hostingEnvironment).EnsureEndsWith('/') + "Views/Preview/" + "Index.cshtml", model);
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
            var user = _umbracoContextAccessor.UmbracoContext.Security.CurrentUser;

            var previewToken = _publishedSnapshotService.EnterPreview(user, id);

            _cookieManager.SetCookieValue(Constants.Web.PreviewCookieName, previewToken);

            // use a numeric url because content may not be in cache and so .Url would fail
            var query = culture.IsNullOrWhiteSpace() ? string.Empty : $"?culture={culture}";
            Response.Redirect($"../../{id}.aspx{query}", true);

            return null;
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
