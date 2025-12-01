using System.Text;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Persistence.Repositories;

[Obsolete("Installation logging is no longer supported and this class will be removed in Umbraco 19.")]
public class InstallationRepository : IInstallationRepository
{

    public InstallationRepository(IJsonSerializer jsonSerializer)
    {

    }

    [Obsolete("This method no longer has any function and will be removed in Umbraco 19.")]
    public Task SaveInstallLogAsync(InstallLog installLog) => Task.CompletedTask;
}
