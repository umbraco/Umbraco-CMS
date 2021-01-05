using System.Threading.Tasks;
using Semver;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Core.Services
{
    public class UpgradeService : IUpgradeService
    {
        private readonly IUpgradeCheckRepository _upgradeCheckRepository;

        public UpgradeService(IUpgradeCheckRepository upgradeCheckRepository)
        {
            _upgradeCheckRepository = upgradeCheckRepository;
        }

        public async Task<UpgradeResult> CheckUpgrade(SemVersion version)
        {
            return await _upgradeCheckRepository.CheckUpgradeAsync(version);
        }
    }
}
