using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for checking for Umbraco upgrades.
/// </summary>
[Obsolete("Upgrade checks are no longer supported and this interface will be removed in Umbraco 19.")]
public interface IUpgradeCheckRepository
{
    /// <summary>
    ///     Checks for available upgrades for the specified version.
    /// </summary>
    /// <param name="version">The current version to check upgrades for.</param>
    /// <returns>The upgrade result containing available upgrade information.</returns>
    [Obsolete("This method no longer has any function and will be removed in Umbraco 19.")]
    Task<UpgradeResult> CheckUpgradeAsync(SemVersion version);
}
