using System;
using System.Linq;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    /// <summary>
    /// This step is purely here to show the button to commence the upgrade
    /// </summary>
    [InstallSetupStep(InstallationType.Upgrade,
        "Upgrade", "upgrade", 1, "Upgrading Umbraco to the latest and greatest version.")]
    internal class UpgradeStep : InstallSetupStep<object>
    {
        private readonly ApplicationContext _appCtx;

        public UpgradeStep(ApplicationContext appCtx)
        {
            _appCtx = appCtx;
        }

        public override bool RequiresExecution(object model)
        {
            return true;
        }

        public override InstallSetupResult Execute(object model)
        {
            return null;
        }

        public override object ViewModel
        {
            get
            {
                var currentVersion = _appCtx.CurrentVersion().GetVersion(3).ToString();
                var newVersion = UmbracoVersion.Current.ToString();
                var reportUrl = string.Format("https://our.umbraco.com/contribute/releases/compare?from={0}&to={1}&notes=1", currentVersion, newVersion);

                return new
                {
                    currentVersion = currentVersion,
                    newVersion = newVersion,
                    reportUrl = reportUrl
                };

            }
        }

    }
}