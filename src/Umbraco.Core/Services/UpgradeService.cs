using System.Threading.Tasks;
using Semver;
using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Services;

namespace Umbraco.Core
{
    [UmbracoVolatile]
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
