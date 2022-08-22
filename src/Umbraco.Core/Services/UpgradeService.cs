using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Core.Services;

public class UpgradeService : IUpgradeService
{
    private readonly IUpgradeCheckRepository _upgradeCheckRepository;

    public UpgradeService(IUpgradeCheckRepository upgradeCheckRepository) =>
        _upgradeCheckRepository = upgradeCheckRepository;

    public async Task<UpgradeResult> CheckUpgrade(SemVersion version) =>
        await _upgradeCheckRepository.CheckUpgradeAsync(version);
}
