using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Cms.Infrastructure.Install;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Install;
/// <summary>
///     The Installation controller
/// </summary>
[Obsolete("Will no longer be required with the new backoffice API")]
[InstallAuthorize]
[Area(Constants.Web.Mvc.InstallArea)]
public class InstallController : Controller
{
    private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
    private readonly GlobalSettings _globalSettings;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly InstallHelper _installHelper;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger<InstallController> _logger;
    private readonly IRuntimeState _runtime;
    private readonly IRuntimeMinifier _runtimeMinifier;
    private readonly IUmbracoVersion _umbracoVersion;

    public InstallController(
        IBackOfficeSecurityAccessor backofficeSecurityAccessor,
        InstallHelper installHelper,
        IRuntimeState runtime,
        IOptions<GlobalSettings> globalSettings,
        IRuntimeMinifier runtimeMinifier,
        IHostingEnvironment hostingEnvironment,
        IUmbracoVersion umbracoVersion,
        ILogger<InstallController> logger,
        LinkGenerator linkGenerator)
    {
        _backofficeSecurityAccessor = backofficeSecurityAccessor;
        _installHelper = installHelper;
        _runtime = runtime;
        _globalSettings = globalSettings.Value;
        _runtimeMinifier = runtimeMinifier;
        _hostingEnvironment = hostingEnvironment;
        _umbracoVersion = umbracoVersion;
        _logger = logger;
        _linkGenerator = linkGenerator;
    }

    [HttpGet]
    [StatusCodeResult(HttpStatusCode.ServiceUnavailable)]
    public async Task<ActionResult> Index()
    {
        // Get the install base URL
        ViewData.SetInstallApiBaseUrl(_linkGenerator.GetInstallerApiUrl());

        // Get the base umbraco folder
        var baseFolder = _hostingEnvironment.ToAbsolute(_globalSettings.UmbracoPath);
        ViewData.SetUmbracoBaseFolder(baseFolder);

        ViewData.SetUmbracoVersion(_umbracoVersion.SemanticVersion);

        await _installHelper.SetInstallStatusAsync(false, string.Empty);

        return View(Path.Combine(Constants.SystemDirectories.Umbraco.TrimStart("~"), Constants.Web.Mvc.InstallArea, nameof(Index) + ".cshtml"));
    }

    [HttpGet]
    [IgnoreFromNotFoundSelectorPolicy]
    public ActionResult Redirect() => NotFound();
}
