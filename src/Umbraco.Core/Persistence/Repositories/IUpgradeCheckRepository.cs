using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IUpgradeCheckRepository
{
    Task<UpgradeResult> CheckUpgradeAsync(SemVersion version);
}
