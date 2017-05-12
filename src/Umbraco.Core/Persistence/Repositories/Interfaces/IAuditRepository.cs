using Umbraco.Core.Models;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IAuditRepository : IUnitOfWorkRepository, IQueryRepository<int, AuditItem>
    {
        void CleanLogs(int maximumAgeOfLogsInMinutes);
    }
}