using Umbraco.Cms.Core.Semver;
using Umbraco.Extensions;

namespace Umbraco.New.Cms.Core.Models.Installer;

public class UpgradeSettingsModel
{
    public string CurrentState { get; set; } = string.Empty;

    public string NewState { get; set; } = string.Empty;

    public SemVersion NewVersion { get; set; } = null!;

    public SemVersion OldVersion { get; set; } = null!;
}
