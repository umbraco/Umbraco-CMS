using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
namespace Umbraco.Cms.Core.Install.InstallSteps
{
    /// <summary>
    /// This step is purely here to show the button to commence the upgrade
    /// </summary>
    [Obsolete("Will be replace with a new step with the new backoffice")]
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

        public override Task<InstallSetupResult?> ExecuteAsync(object model) => Task.FromResult<InstallSetupResult?>(null);

        public override object ViewModel
        {
            get
            {
                string FormatGuidState(string? value)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        value = "unknown";
                    }
                    else if (Guid.TryParse(value, out Guid currentStateGuid))
                    {
                        value = currentStateGuid.ToString("N").Substring(0, 8);
                    }

                    return value;
                }

                var currentState = FormatGuidState(_runtimeState.CurrentMigrationState);
                var newState = FormatGuidState(_runtimeState.FinalMigrationState);
                var newVersion = _umbracoVersion.SemanticVersion?.ToSemanticStringWithoutBuild();
                var oldVersion = new SemVersion(_umbracoVersion.SemanticVersion?.Major ?? 0).ToString(); //TODO can we find the old version somehow? e.g. from current state

                var reportUrl = $"https://our.umbraco.com/contribute/releases/compare?from={oldVersion}&to={newVersion}&notes=1";

                return new { oldVersion, newVersion, currentState, newState, reportUrl };
            }
        }
    }
}
