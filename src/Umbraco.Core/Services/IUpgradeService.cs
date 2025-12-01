using Umbraco.Cms.Core.Semver;

namespace Umbraco.Cms.Core.Services;

[Obsolete("Upgrade checks are no longer supported and this service will be removed in Umbraco 19.")]
public interface IUpgradeService
{
    [Obsolete("This method no longer has any function and will be removed in Umbraco 19.")]
    Task<UpgradeResult> CheckUpgrade(SemVersion version);
}
