using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.DataLayer;

namespace umbraco.BusinessLogic.Interfaces
{
    public interface ILog
    {
        void Add(LogTypes logType, User user, int nodeId, string comment);
        void Add(Exception exception);
        void CleanLogs(int maximumAgeOfLogsInMinutes);
        List<LogItem> GetLogItems(LogTypes Type, DateTime SinceDate);
        List<LogItem> GetLogItems(int NodeId);
        List<LogItem> GetLogItems(User user, DateTime SinceDate);
        List<LogItem> GetLogItems(User user, LogTypes Type, DateTime SinceDate);
        List<LogItem> GetAuditLogReader(int NodeId);
    }
}
