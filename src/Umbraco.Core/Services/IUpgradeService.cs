using System.Threading.Tasks;
using Semver;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IUpgradeService
    {
        Task<UpgradeResult> CheckUpgrade(SemVersion version);
    }
}
