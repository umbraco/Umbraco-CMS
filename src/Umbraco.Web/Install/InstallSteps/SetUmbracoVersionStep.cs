using System.Threading.Tasks;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly InstallHelper _installHelper;
        private readonly IGlobalSettings _globalSettings;
        private readonly IUserService _userService;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IIOHelper _ioHelper;

        public SetUmbracoVersionStep(IHttpContextAccessor httpContextAccessor, InstallHelper installHelper, IGlobalSettings globalSettings, IUserService userService, IUmbracoVersion umbracoVersion, IIOHelper ioHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _installHelper = installHelper;
            _globalSettings = globalSettings;
            _userService = userService;
            _umbracoVersion = umbracoVersion;
            _ioHelper = ioHelper;
        }

        public override Task<InstallSetupResult> ExecuteAsync(object model)
        {
            var security = new WebSecurity(_httpContextAccessor, _userService, _globalSettings, _ioHelper);

            if (security.IsAuthenticated() == false && _globalSettings.ConfigurationStatus.IsNullOrWhiteSpace())
            {
                security.PerformLogin(-1);
            }

            if (security.IsAuthenticated())
            {
                // when a user is already logged in, we need to check whether it's user 'zero'
                // which is the legacy super user from v7 - and then we need to actually log the
                // true super user in - but before that we need to log off, else audit events
                // will try to reference user zero and fail
                var userIdAttempt = security.GetUserId();
                if (userIdAttempt && userIdAttempt.Result == 0)
                {
                    security.ClearCurrentLogin();
                    security.PerformLogin(Constants.Security.SuperUserId);
                }
            }
            else if (_globalSettings.ConfigurationStatus.IsNullOrWhiteSpace())
            {
                // for installs, we need to log the super user in
                security.PerformLogin(Constants.Security.SuperUserId);
            }

            // Update configurationStatus
            _globalSettings.ConfigurationStatus = _umbracoVersion.SemanticVersion.ToSemanticString();

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
