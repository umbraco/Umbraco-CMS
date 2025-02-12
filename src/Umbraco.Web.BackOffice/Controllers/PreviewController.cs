using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.Features;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Cms.Infrastructure.WebAssets;
using Umbraco.Cms.Web.BackOffice.ActionResults;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers;

[DisableBrowserCache]
[Area(Constants.Web.Mvc.BackOfficeArea)]
public partial class PreviewController : Controller
{
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly ICookieManager _cookieManager;
    private readonly UmbracoFeatures _features;
    private readonly GlobalSettings _globalSettings;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILocalizationService _localizationService;
    private readonly IPublishedSnapshotService _publishedSnapshotService;
    private readonly IRuntimeMinifier _runtimeMinifier;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly ICompositeViewEngine _viewEngines;
    private readonly WebRoutingSettings _webRoutingSettings;

    [Obsolete("Please use the non-obsolete constructor.")]
    public PreviewController(
        UmbracoFeatures features,
        IOptionsSnapshot<GlobalSettings> globalSettings,
        IPublishedSnapshotService publishedSnapshotService,
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        ILocalizationService localizationService,
        IHostingEnvironment hostingEnvironment,
        ICookieManager cookieManager,
        IRuntimeMinifier runtimeMinifier,
        ICompositeViewEngine viewEngines,
        IUmbracoContextAccessor umbracoContextAccessor)
        : this(
            features,
            globalSettings,
            StaticServiceProvider.Instance.GetRequiredService<IOptionsSnapshot<WebRoutingSettings>>(),
            publishedSnapshotService,
            backofficeSecurityAccessor,
            localizationService,
            hostingEnvironment,
            cookieManager,
            runtimeMinifier,
            viewEngines,
            umbracoContextAccessor)
    {
    }

    [ActivatorUtilitiesConstructor]
    public PreviewController(
        UmbracoFeatures features,
        IOptionsSnapshot<GlobalSettings> globalSettings,
        IOptionsSnapshot<WebRoutingSettings> webRoutingSettings,
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
        _webRoutingSettings = webRoutingSettings.Value;
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
        IEnumerable<ILanguage> availableLanguages = _localizationService.GetAllLanguages();
        if (id.HasValue)
        {
            IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
            IPublishedContent? content = umbracoContext.Content?.GetById(true, id.Value);
            if (content is null)
            {
                return NotFound();
            }

            availableLanguages = availableLanguages.Where(language => content.Cultures.ContainsKey(language.IsoCode));
        }

        var model = new BackOfficePreviewModel(_features, availableLanguages);

        if (model.PreviewExtendedHeaderView.IsNullOrWhiteSpace() == false)
        {
            ViewEngineResult viewEngineResult =
                _viewEngines.FindView(ControllerContext, model.PreviewExtendedHeaderView!, false);
            if (viewEngineResult.View == null)
            {
                throw new InvalidOperationException("Could not find the view " + model.PreviewExtendedHeaderView +
                                                    ", the following locations were searched: " + Environment.NewLine +
                                                    string.Join(Environment.NewLine,
                                                        viewEngineResult.SearchedLocations));
            }
        }

        var viewPath = Path.Combine(
                _globalSettings.UmbracoPath,
                Constants.Web.Mvc.BackOfficeArea,
                ControllerExtensions.GetControllerName<PreviewController>() + ".cshtml")
            .Replace("\\", "/"); // convert to forward slashes since it's a virtual path

        return View(viewPath, model);
    }


    /// <summary>
    ///     Returns the JavaScript file for preview
    /// </summary>
    /// <returns></returns>
    [MinifyJavaScriptResult(Order = 0)]
    // TODO: Replace this with response caching https://docs.microsoft.com/en-us/aspnet/core/performance/caching/response?view=aspnetcore-3.1
    //[OutputCache(Order = 1, VaryByParam = "none", Location = OutputCacheLocation.Server, Duration = 5000)]
    public async Task<JavaScriptResult> Application()
    {
        IEnumerable<string> files =
            await _runtimeMinifier.GetJsAssetPathsAsync(BackOfficeWebAssets.UmbracoPreviewJsBundleName);
        var result =
            BackOfficeJavaScriptInitializer.GetJavascriptInitialization(files, "umbraco.preview", _globalSettings,
                _hostingEnvironment);

        return new JavaScriptResult(result);
    }

    /// <summary>
    ///     The endpoint that is loaded within the preview iframe
    /// </summary>
    [Authorize(Policy = AuthorizationPolicies.BackOfficeAccess)]
    public ActionResult Frame(int id, string culture)
    {
        if (ValidateProvidedCulture(culture) is false)
        {
            throw new InvalidOperationException($"Could not recognise the provided culture: {culture}");
        }

        EnterPreview(id);

        // use a numeric URL because content may not be in cache and so .Url would fail
        var query = culture.IsNullOrWhiteSpace() ? string.Empty : $"?culture={culture}";

        return RedirectPermanent($"../../{id}{query}");
    }

    private static bool ValidateProvidedCulture(string culture)
    {
        if (string.IsNullOrEmpty(culture))
        {
            return true;
        }

        // We can be confident the backoffice will have provided a valid culture in linking to the
        // preview, so we don't need to check that the culture matches an Umbraco language.
        // We are only concerned here with protecting against XSS attacks from a fiddled preview
        // URL, so we can just confirm we have a valid culture.
        try
        {
            CultureInfo.GetCultureInfo(culture, true);
            return true;
        }
        catch (CultureNotFoundException)
        {
            return false;
        }
    }

    public ActionResult? EnterPreview(int id)
    {
        IUser? user = _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
        _cookieManager.SetCookieValue(Constants.Web.PreviewCookieName, "preview");

        return new EmptyResult();
    }

    public ActionResult End(string? redir = null)
    {
        _cookieManager.ExpireCookie(Constants.Web.PreviewCookieName);

        // Expire Client-side cookie that determines whether the user has accepted to be in Preview Mode when visiting the website.
        _cookieManager.ExpireCookie(Constants.Web.AcceptPreviewCookieName);

        // are we attempting a redirect to the default route (by ID with optional culture)?
        Match match = DefaultPreviewRedirectRegex().Match(redir ?? string.Empty);
        if (match.Success)
        {
            var id = int.Parse(match.Groups["id"].Value);

            // first try to resolve the published URL
            if (_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext) &&
                umbracoContext.Content is not null)
            {
                IPublishedContent? publishedContent = umbracoContext.Content.GetById(id);
                if (publishedContent is null)
                {
                    // content is not published, redirect to root
                    return Redirect("/");
                }

                var culture = publishedContent.ContentType.VariesByCulture()
                              && match.Groups.TryGetValue("culture", out Group? group)
                    ? group.Value
                    : null;

                var publishedUrl = publishedContent.Url(culture);
                if (WebPath.IsWellFormedWebPath(publishedUrl, UriKind.RelativeOrAbsolute))
                {
                    return Redirect(publishedUrl);
                }
            }

            // could not resolve the published URL - are we allowed to route content by ID?
            if (_webRoutingSettings.DisableFindContentByIdPath)
            {
                // no we are not - redirect to root instead
                return Redirect("/");
            }
        }

        if (WebPath.IsWellFormedWebPath(redir, UriKind.Relative)
            && Uri.TryCreate(redir, UriKind.Relative, out Uri? url))
        {
            return Redirect(url.ToString());
        }

        return Redirect("/");
    }

    [GeneratedRegex("^\\/(?<id>\\d*)(\\?culture=(?<culture>[\\w-]*))?$")]
    private static partial Regex DefaultPreviewRedirectRegex();
}
