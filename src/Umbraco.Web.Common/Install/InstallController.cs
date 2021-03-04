﻿using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
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
using Umbraco.Extensions;
using Umbraco.Cms.Web.Common.Filters;

namespace Umbraco.Cms.Web.Common.Install
{

    /// <summary>
    /// The Installation controller
    /// </summary>
    [InstallAuthorize]
    [Area(Cms.Core.Constants.Web.Mvc.InstallArea)]
    public class InstallController : Controller
    {
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly InstallHelper _installHelper;
        private readonly IRuntimeState _runtime;
        private readonly GlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly ILogger<InstallController> _logger;
        private readonly LinkGenerator _linkGenerator;
        private readonly IRuntimeMinifier _runtimeMinifier;

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
        [StatusCodeResult(System.Net.HttpStatusCode.ServiceUnavailable)]
        [TypeFilter(typeof(StatusCodeResultAttribute), Arguments = new object []{System.Net.HttpStatusCode.ServiceUnavailable})]
        public async Task<ActionResult> Index()
        {
            var umbracoPath = Url.GetBackOfficeUrl();

            if (_runtime.Level == RuntimeLevel.Run)
                return Redirect(umbracoPath);

            if (_runtime.Level == RuntimeLevel.Upgrade)
            {
                // Update ClientDependency version and delete its temp directories to make sure we get fresh caches
                _runtimeMinifier.Reset();

                var authResult = await this.AuthenticateBackOfficeAsync();

                if (!authResult.Succeeded)
                {
                    return Redirect(_globalSettings.UmbracoPath + "/AuthorizeUpgrade?redir=" + Request.GetEncodedUrl());
                }
            }

            // gen the install base URL
            ViewData.SetInstallApiBaseUrl(_linkGenerator.GetInstallerApiUrl());

            // get the base umbraco folder
            var baseFolder = _hostingEnvironment.ToAbsolute(_globalSettings.UmbracoPath);
            ViewData.SetUmbracoBaseFolder(baseFolder);

            ViewData.SetUmbracoVersion(_umbracoVersion.SemanticVersion);

            await _installHelper.SetInstallStatusAsync(false, "");

            return View(Path.Combine(baseFolder , Cms.Core.Constants.Web.Mvc.InstallArea, nameof(Index) + ".cshtml"));
        }

        /// <summary>
        /// Used to perform the redirect to the installer when the runtime level is <see cref="RuntimeLevel.Install"/> or <see cref="RuntimeLevel.Upgrade"/>
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Redirect()
        {
            var uri = HttpContext.Request.GetEncodedUrl();

            // redirect to install
            ReportRuntime(_logger, _runtime.Level, "Umbraco must install or upgrade.");

            var installUrl = $"{_linkGenerator.GetInstallerUrl()}?redir=true&url={uri}";
            return Redirect(installUrl);
        }

        private static bool _reported;
        private static RuntimeLevel _reportedLevel;

        private static void ReportRuntime(ILogger<InstallController> logger, RuntimeLevel level, string message)
        {
            if (_reported && _reportedLevel == level) return;
            _reported = true;
            _reportedLevel = level;
            logger.LogWarning(message);
        }
    }
}
