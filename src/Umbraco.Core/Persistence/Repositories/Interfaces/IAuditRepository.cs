using System.Collections;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IAuditRepository : IRepositoryQueryable<int, AuditItem>
    {
        void CleanLogs(int maximumAgeOfLogsInMinutes);
    }
}