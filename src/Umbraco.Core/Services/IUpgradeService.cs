using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Provides functionality for checking available Umbraco upgrades.
/// </summary>
[Obsolete("Upgrade checks are no longer supported and this service will be removed in Umbraco 19.")]
public interface IUpgradeService
{
    /// <summary>
    /// Checks if an upgrade is available for the specified version.
    /// </summary>
    /// <param name="version">The current semantic version to check for upgrades.</param>
    /// <returns>The result of the upgrade check.</returns>
    [Obsolete("This method no longer has any function and will be removed in Umbraco 19.")]
    Task<UpgradeResult> CheckUpgrade(SemVersion version);
}
