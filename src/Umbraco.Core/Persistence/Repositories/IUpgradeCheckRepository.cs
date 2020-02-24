using System.Threading.Tasks;
using Semver;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IUpgradeCheckRepository
    {
        Task<UpgradeResult> CheckUpgradeAsync(SemVersion version);
    }
}
