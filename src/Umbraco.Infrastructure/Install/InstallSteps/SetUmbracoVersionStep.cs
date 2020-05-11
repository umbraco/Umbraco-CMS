using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Net;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall | InstallationType.Upgrade,
        "UmbracoVersion", 50, "Installation is complete! Get ready to be redirected to your new CMS.",
        PerformsAppRestart = true)]
    public class SetUmbracoVersionStep : InstallSetupStep<object>
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly InstallHelper _installHelper;
        private readonly IGlobalSettings _globalSettings;
        private readonly IUmbracoVersion _umbracoVersion;

        public SetUmbracoVersionStep(IUmbracoContextAccessor umbracoContextAccessor, InstallHelper installHelper,
            IGlobalSettings globalSettings, IUmbracoVersion umbracoVersion)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _installHelper = installHelper;
            _globalSettings = globalSettings;
            _umbracoVersion = umbracoVersion;
        }

        public override Task<InstallSetupResult> ExecuteAsync(object model)
        {
            //TODO: This needs to be reintroduced, when users are compatible with ASP.NET Core Identity.
            // var security = _umbracoContextAccessor.GetRequiredUmbracoContext().Security;
            // if (security.IsAuthenticated() == false && _globalSettings.ConfigurationStatus.IsNullOrWhiteSpace())
            // {
            //     security.PerformLogin(-1);
            // }
            //
            // if (security.IsAuthenticated())
            // {
            //     // when a user is already logged in, we need to check whether it's user 'zero'
            //     // which is the legacy super user from v7 - and then we need to actually log the
            //     // true super user in - but before that we need to log off, else audit events
            //     // will try to reference user zero and fail
            //     var userIdAttempt = security.GetUserId();
            //     if (userIdAttempt && userIdAttempt.Result == 0)
            //     {
            //         security.ClearCurrentLogin();
            //         security.PerformLogin(Constants.Security.SuperUserId);
            //     }
            // }
            // else if (_globalSettings.ConfigurationStatus.IsNullOrWhiteSpace())
            // {
            //     // for installs, we need to log the super user in
            //     security.PerformLogin(Constants.Security.SuperUserId);
            // }

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
