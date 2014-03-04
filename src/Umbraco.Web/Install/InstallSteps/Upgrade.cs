using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    /// <summary>
    /// This step is purely here to show the button to commence the upgrade
    /// </summary>
    [InstallSetupStep(InstallationType.Upgrade,
        "Upgrade", "upgrade", 1, "Upgrading umbraco")]
    internal class Upgrade : InstallSetupStep<object>
    {
        public override bool RequiresExecution()
        {
            return true;
        }

        public override InstallSetupResult Execute(object model)
        {
            return null;
        }
    }
}