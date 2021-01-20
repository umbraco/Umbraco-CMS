using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
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
using Microsoft.AspNetCore.Authorization;
using Umbraco.Web.Common.Authorization;

namespace Umbraco.Web.BackOffice.Controllers
{
    [DisableBrowserCache]
    [Area(Constants.Web.Mvc.BackOfficeArea)]
    public class PreviewController : Controller
    {
        private readonly UmbracoFeatures _features;
        private readonly GlobalSettings _globalSettings;
        private readonly IPublishedSnapshotService _publishedSnapshotService;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ICookieManager _cookieManager;
        private readonly IRuntimeMinifier _runtimeMinifier;
        private readonly ICompositeViewEngine _viewEngines;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public PreviewController(
            UmbracoFeatures features,
            IOptions<GlobalSettings> globalSettings,
            IPublishedSnapshotService publishedSnapshotService,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            ILocalizationService localizationService,
            IHostingEnvironment hostingEnvironment,
            ICookieManager cookieManager,
            IRuntimeMinifier runtimeMinifier,
            ICompositeViewEngine viewEngines,
            IUmbracoContextAccessor umbracoContextAccessor)
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
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        [Authorize(Policy = AuthorizationPolicies.BackOfficeAccessWithoutApproval)]
        [DisableBrowserCache]
        public ActionResult Index(int? id = null)
        {
            var availableLanguages = _localizationService.GetAllLanguages();
            if (id.HasValue)
            {
                var content = _umbracoContextAccessor.UmbracoContext.Content.GetById(true, id.Value);
                if (content is null)
                    return NotFound();

                availableLanguages = availableLanguages.Where(language => content.Cultures.ContainsKey(language.IsoCode));
            }
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
        [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
        public ActionResult Frame(int id, string culture)
        {
            EnterPreview(id);

            // use a numeric URL because content may not be in cache and so .Url would fail
            var query = culture.IsNullOrWhiteSpace() ? string.Empty : $"?culture={culture}";

            return RedirectPermanent($"../../{id}{query}");
        }

        public ActionResult EnterPreview(int id)
        {
            var user = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser;
            _cookieManager.SetCookieValue(Constants.Web.PreviewCookieName, "preview");

            return null;
        }

        public ActionResult End(string redir = null)
        {
            _cookieManager.ExpireCookie(Constants.Web.PreviewCookieName);

            // Expire Client-side cookie that determines whether the user has accepted to be in Preview Mode when visiting the website.
            _cookieManager.ExpireCookie(Constants.Web.AcceptPreviewCookieName);

            if (Uri.IsWellFormedUriString(redir, UriKind.Relative)
                && redir.StartsWith("//") == false
                && Uri.TryCreate(redir, UriKind.Relative, out var url))
                return Redirect(url.ToString());

            return Redirect("/");
        }
    }
}
