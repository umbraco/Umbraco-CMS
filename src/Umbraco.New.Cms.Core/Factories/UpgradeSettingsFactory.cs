using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Services;
using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Core.Factories;

public class UpgradeSettingsFactory : IUpgradeSettingsFactory
{
    private readonly IRuntimeState _runtimeState;
    private readonly IUmbracoVersion _umbracoVersion;

    public UpgradeSettingsFactory(
        IRuntimeState runtimeState,
        IUmbracoVersion umbracoVersion)
    {
        _runtimeState = runtimeState;
        _umbracoVersion = umbracoVersion;
    }


    public UpgradeSettingsModel GetUpgradeSettings()
    {
        var model = new UpgradeSettingsModel
        {
            CurrentState = _runtimeState.CurrentMigrationState ?? string.Empty,
            NewState = _runtimeState.FinalMigrationState ?? string.Empty,
            NewVersion = _umbracoVersion.SemanticVersion,
            OldVersion = new SemVersion(_umbracoVersion.SemanticVersion.Major), // TODO can we find the old version somehow? e.g. from current state
        };

        return model;
    }
}
