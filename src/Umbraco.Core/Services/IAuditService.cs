using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    public interface IAuditService : IService
    {
        void Add(AuditType type, string comment, int userId, int objectId);
        IEnumerable<AuditItem> GetLogs(int objectId);
        IEnumerable<AuditItem> GetUserLogs(int userId, AuditType type, DateTime? sinceDate = null);
        IEnumerable<AuditItem> GetLogs(AuditType type, DateTime? sinceDate = null);
        void CleanLogs(int maximumAgeOfLogsInMinutes);
    }
}
