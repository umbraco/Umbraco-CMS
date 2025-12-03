using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Core.Services;

[Obsolete("Upgrade checks are no longer supported and this service will be removed in Umbraco 19.")]
public class UpgradeService : IUpgradeService
{
    private readonly IUpgradeCheckRepository _upgradeCheckRepository;

    public UpgradeService(IUpgradeCheckRepository upgradeCheckRepository) =>
        _upgradeCheckRepository = upgradeCheckRepository;

    public async Task<UpgradeResult> CheckUpgrade(SemVersion version) =>
        await _upgradeCheckRepository.CheckUpgradeAsync(version);
}
