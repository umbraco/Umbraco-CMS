using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides functionality for checking for available Umbraco upgrades.
/// </summary>
[Obsolete("Upgrade checks are no longer supported and this service will be removed in Umbraco 19.")]
public class UpgradeService : IUpgradeService
{
    private readonly IUpgradeCheckRepository _upgradeCheckRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UpgradeService" /> class.
    /// </summary>
    /// <param name="upgradeCheckRepository">The repository for checking upgrades.</param>
    public UpgradeService(IUpgradeCheckRepository upgradeCheckRepository) =>
        _upgradeCheckRepository = upgradeCheckRepository;

    /// <inheritdoc />
    public async Task<UpgradeResult> CheckUpgrade(SemVersion version) =>
        await _upgradeCheckRepository.CheckUpgradeAsync(version);
}
