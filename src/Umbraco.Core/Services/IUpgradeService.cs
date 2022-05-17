using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Core.Services;

public interface IUpgradeService
{
    Task<UpgradeResult> CheckUpgrade(SemVersion version);
}
