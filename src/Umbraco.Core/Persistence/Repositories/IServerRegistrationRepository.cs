using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IServerRegistrationRepository : IReadWriteQueryRepository<int, IServerRegistration>
    {
        void DeactiveStaleServers(TimeSpan staleTimeout);
    }
}
