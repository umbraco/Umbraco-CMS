using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models.Installer;

namespace Umbraco.Cms.Core.Factories;

/// <summary>
/// Factory for creating <see cref="UpgradeSettingsModel"/> instances containing upgrade-related settings.
/// </summary>
public class UpgradeSettingsFactory : IUpgradeSettingsFactory
{
    private readonly IRuntimeState _runtimeState;
    private readonly IUmbracoVersion _umbracoVersion;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpgradeSettingsFactory"/> class.
    /// </summary>
    /// <param name="runtimeState">The runtime state providing migration information.</param>
    /// <param name="umbracoVersion">The Umbraco version information.</param>
    public UpgradeSettingsFactory(
        IRuntimeState runtimeState,
        IUmbracoVersion umbracoVersion)
    {
        _runtimeState = runtimeState;
        _umbracoVersion = umbracoVersion;
    }

    /// <inheritdoc />
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
