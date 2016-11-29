using System.Collections;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IAuditRepository : IQueryRepository<int, AuditItem>
    {
        void CleanLogs(int maximumAgeOfLogsInMinutes);
    }
}