using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Core.Persistence.Repositories;

[Obsolete("Upgrade checks are no longer supported and this interface will be removed in Umbraco 19.")]
public interface IUpgradeCheckRepository
{
    [Obsolete("This method no longer has any function and will be removed in Umbraco 19.")]
    Task<UpgradeResult> CheckUpgradeAsync(SemVersion version);
}
