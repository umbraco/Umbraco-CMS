using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IAuditRepository : IReadRepository<int, AuditItem>, IWriteRepository<AuditItem>, IQueryRepository<AuditItem>
    {
        void CleanLogs(int maximumAgeOfLogsInMinutes);
    }
}
