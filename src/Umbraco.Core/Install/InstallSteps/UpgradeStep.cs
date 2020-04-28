using System;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    /// <summary>
    /// This step is purely here to show the button to commence the upgrade
    /// </summary>
    [InstallSetupStep(InstallationType.Upgrade, "Upgrade", "upgrade", 1, "Upgrading Umbraco to the latest and greatest version.")]
    public class UpgradeStep : InstallSetupStep<object>
    {
        public override bool RequiresExecution(object model) => true;
        private readonly IUmbracoVersion _umbracoVersion;
        private readonly IRuntimeState _runtimeState;

        public UpgradeStep(IUmbracoVersion umbracoVersion, IRuntimeState runtimeState)
        {
            _umbracoVersion = umbracoVersion;
            _runtimeState = runtimeState;
        }

        public override Task<InstallSetupResult> ExecuteAsync(object model) => Task.FromResult<InstallSetupResult>(null);

        public override object ViewModel
        {
            get
            {
                //TODO this will always compare the same version now
                var newVersion = _umbracoVersion.SemanticVersion.ToString();

                string FormatGuidState(string value)
                {
                    if (string.IsNullOrWhiteSpace(value)) value = "unknown";
                    else if (Guid.TryParse(value, out var currentStateGuid))
                        value = currentStateGuid.ToString("N").Substring(0, 8);
                    return value;
                }


                var currentState = FormatGuidState(_runtimeState.CurrentMigrationState);
                var newState = FormatGuidState(_runtimeState.FinalMigrationState);
                var currentVersion = _umbracoVersion.Current;

                var reportUrl = $"https://our.umbraco.com/contribute/releases/compare?from={currentVersion}&to={newVersion}&notes=1";

                return new { currentVersion, newVersion, currentState, newState, reportUrl };
            }
        }
    }
}
