using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Persistence.Repositories;

[Obsolete("Upgrade checks are no longer supported and this repository will be removed in Umbraco 19.")]
public class UpgradeCheckRepository : IUpgradeCheckRepository
{
    public UpgradeCheckRepository(IJsonSerializer jsonSerializer)
    {

    }

    [Obsolete("This method no longer has any function and will be removed in Umbraco 19.")]
    public Task<UpgradeResult> CheckUpgradeAsync(SemVersion version) => Task.FromResult(new UpgradeResult("None", string.Empty, string.Empty));
}
