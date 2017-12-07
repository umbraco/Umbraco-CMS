using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IAuditRepository : IUnitOfWorkRepository, IReadRepository<int, AuditItem>, IWriteRepository<AuditItem>, IQueryRepository<AuditItem>
    {
        void CleanLogs(int maximumAgeOfLogsInMinutes);
    }
}
