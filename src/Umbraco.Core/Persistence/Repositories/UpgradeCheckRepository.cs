using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Provides an implementation of <see cref="IUpgradeCheckRepository" /> for checking for Umbraco upgrades.
/// </summary>
[Obsolete("Upgrade checks are no longer supported and this repository will be removed in Umbraco 19.")]
public class UpgradeCheckRepository : IUpgradeCheckRepository
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UpgradeCheckRepository" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public UpgradeCheckRepository(IJsonSerializer jsonSerializer)
    {

    }

    /// <inheritdoc />
    [Obsolete("This method no longer has any function and will be removed in Umbraco 19.")]
    public Task<UpgradeResult> CheckUpgradeAsync(SemVersion version) => Task.FromResult(new UpgradeResult("None", string.Empty, string.Empty));
}
