using System;
using Umbraco.Core.Composing;
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
        public override bool RequiresExecution(object model) => true;

        public override InstallSetupResult Execute(object model) => null;

        public override object ViewModel
        {
            get
            {
                var currentVersion = UmbracoVersion.Local;

                //fixme - in this case there's a db but the version is cleared which is fine and a normal way to force the upgrader
                // to execute, but before we would detect the current version via the DB like DatabaseSchemaResult.DetermineInstalledVersion
                // what now, do we need to?
                if (currentVersion == null)
                    currentVersion = new Semver.SemVersion(0); 

                var newVersion = UmbracoVersion.SemanticVersion.ToString();

                string FormatGuidState(string value)
                {
                    if (string.IsNullOrWhiteSpace(value)) value = "unknown";
                    else if (Guid.TryParse(value, out var currentStateGuid))
                        value = currentStateGuid.ToString("N").Substring(0, 8);
                    return value;
                }

                var state = Current.RuntimeState; // fixme inject
                var currentState = FormatGuidState(state.CurrentMigrationState);
                var newState = FormatGuidState(state.FinalMigrationState);

                var reportUrl = $"https://our.umbraco.com/contribute/releases/compare?from={currentVersion}&to={newVersion}&notes=1";

                return new { currentVersion, newVersion, currentState, newState, reportUrl };
            }
        }
    }
}
