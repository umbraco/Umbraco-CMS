using Umbraco.Core.Configuration;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    /// <summary>
    /// This step is purely here to show the button to commence the upgrade
    /// </summary>
    [InstallSetupStep(InstallationType.Upgrade, "Upgrade", "upgrade", 1, "Upgrading Umbraco to the latest and greatest version.")]
    internal class UpgradeStep : InstallSetupStep<object>
    {
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
                // fixme where is the "detected current version"?
                var currentVersion = UmbracoVersion.Local.ToString();
                var newVersion = UmbracoVersion.Current.ToString();
                var reportUrl = $"https://our.umbraco.org/contribute/releases/compare?from={currentVersion}&to={newVersion}&notes=1";

                return new { currentVersion, newVersion, reportUrl };
            }
        }
    }
}
