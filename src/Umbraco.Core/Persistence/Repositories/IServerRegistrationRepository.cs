using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IServerRegistrationRepository : IReadWriteQueryRepository<int, IServerRegistration>
{
    void DeactiveStaleServers(TimeSpan staleTimeout);

    void ClearCache();
}
