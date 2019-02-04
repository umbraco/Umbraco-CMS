using System.Threading.Tasks;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Cache;
using Umbraco.Web.Composing;
using Umbraco.Web.Install.Models;
using Umbraco.Web.Security;


namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall | InstallationType.Upgrade,
        "UmbracoVersion", 50, "Installation is complete!, get ready to be redirected to your new CMS.",
        PerformsAppRestart = true)]
    internal class SetUmbracoVersionStep : InstallSetupStep<object>
    {
        private readonly HttpContextBase _httpContext;
        private readonly InstallHelper _installHelper;
        private readonly IGlobalSettings _globalSettings;
        private readonly IUserService _userService;
        private readonly DistributedCache _distributedCache;

        public SetUmbracoVersionStep(HttpContextBase httpContext, InstallHelper installHelper, IGlobalSettings globalSettings, IUserService userService, DistributedCache distributedCache)
        {
            _httpContext = httpContext;
            _installHelper = installHelper;
            _globalSettings = globalSettings;
            _userService = userService;
            _distributedCache = distributedCache;
        }

        public override Task<InstallSetupResult> ExecuteAsync(object model)
        {
            //During a new install we'll log the default user in (which is id = 0).
            // During an upgrade, the user will already need to be logged in order to run the installer.

            var security = new WebSecurity(_httpContext, _userService, _globalSettings);
            //we do this check here because for upgrades the user will already be logged in, for brand new installs,
            // they will not be logged in, however we cannot check the current installation status because it will tell
            // us that it is in 'upgrade' because we already have a database conn configured and a database.
            if (security.IsAuthenticated() == false && _globalSettings.ConfigurationStatus.IsNullOrWhiteSpace())
            {
                security.PerformLogin(-1);
            }

            // Some upgrade scripts "may modify the database (cmsContentXml...) tables directly" - not sure
            // that is still true but the idea is that after an upgrade we want to reset the local published snapshot, on
            // all LB nodes of course, so we need to use the distributed cache, and refresh everything.
            _distributedCache.RefreshAllPublishedSnapshot();

            // Update configurationStatus
            _globalSettings.ConfigurationStatus = UmbracoVersion.SemanticVersion.ToSemanticString();

            //reports the ended install
            _installHelper.InstallStatus(true, "");

            return Task.FromResult<InstallSetupResult>(null);
        }

        public override bool RequiresExecution(object model)
        {
            return true;
        }
    }
}
