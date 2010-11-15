using System;
using System.Data;
using System.Diagnostics;
using System.Threading;

using umbraco.DataLayer;
using System.Collections.Generic;
using System.Reflection;
using umbraco.BusinessLogic.Utils;

namespace umbraco.BusinessLogic
{
    /// <summary>
    /// Summary description for Log.
    /// </summary>
    public class Log
    {
        #region Statics
        private Interfaces.ILog m_externalLogger = null;
        private bool m_externalLoggerInitiated = false;

        internal Interfaces.ILog ExternalLogger
        {
            get
            {
                if (!m_externalLoggerInitiated)
                {
                    m_externalLoggerInitiated = true;
                    if (!String.IsNullOrEmpty(UmbracoSettings.ExternalLoggerAssembly)
                         && !String.IsNullOrEmpty(UmbracoSettings.ExternalLoggerType))
                    {
                        try
                        {
                            string assemblyPath = IO.IOHelper.MapPath(UmbracoSettings.ExternalLoggerAssembly);
                            m_externalLogger = Assembly.LoadFrom(assemblyPath).CreateInstance(UmbracoSettings.ExternalLoggerType) as Interfaces.ILog;
                        }
                        catch (Exception ee)
                        {
                            Log.AddLocally(LogTypes.Error, User.GetUser(0), -1,
                                "Error loading external logger: " + ee.ToString());
                        }
                    }
                }

                return m_externalLogger;
            }
        }


        #region Singleton

        public static Log Instance
        {
            get { return Singleton<Log>.Instance; }
        }

        #endregion

        private static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        /// <summary>
        /// Adds the specified log item to the log.
        /// </summary>
        /// <param name="type">The log type.</param>
        /// <param name="user">The user adding the item.</param>
        /// <param name="nodeId">The affected node id.</param>
        /// <param name="comment">Comment.</param>
        public static void Add(LogTypes type, User user, int nodeId, string comment)
        {
            if (Instance.ExternalLogger != null)
            {
                Instance.ExternalLogger.Add(type, user, nodeId, comment);

                // Audit trail too?
                if (!UmbracoSettings.ExternalLoggerLogAuditTrail && type.GetType().GetField(type.ToString()).GetCustomAttributes(typeof(AuditTrailLogItem), true) != null)
                {
                    AddLocally(type, user, nodeId, comment);
                }
            }
            else
            {
                if (!UmbracoSettings.EnableLogging) return;

                if (UmbracoSettings.DisabledLogTypes != null &&
                    UmbracoSettings.DisabledLogTypes.SelectSingleNode(String.Format("//logTypeAlias [. = '{0}']", type.ToString().ToLower())) == null)
                {

                    if (comment.Length > 3999)
                        comment = comment.Substring(0, 3955) + "...";

                    if (UmbracoSettings.EnableAsyncLogging)
                    {
                        ThreadPool.QueueUserWorkItem(
                            delegate { AddSynced(type, user == null ? 0 : user.Id, nodeId, comment); });
                        return;
                    }

                    AddSynced(type, user == null ? 0 : user.Id, nodeId, comment);
                }
            }
        }

        public void AddException(Exception ee)
        {
            if (ExternalLogger != null)
            {
                ExternalLogger.Add(ee);
            }
            else
            {
                Exception ex2 = ee;
                string error = String.Empty;
                string errorMessage = string.Empty;
                while (ex2 != null)
                {
                    error += ex2.ToString();
                    ex2 = ex2.InnerException;
                }
                Add(LogTypes.Error, -1, error);
            }
        }

        /// <summary>
        /// Adds the specified log item to the Umbraco log no matter if an external logger has been defined.
        /// </summary>
        /// <param name="type">The log type.</param>
        /// <param name="user">The user adding the item.</param>
        /// <param name="nodeId">The affected node id.</param>
        /// <param name="comment">Comment.</param>
        public static void AddLocally(LogTypes type, User user, int nodeId, string comment)
        {
            if (comment.Length > 3999)
                comment = comment.Substring(0, 3955) + "...";

            if (UmbracoSettings.EnableAsyncLogging)
            {
                ThreadPool.QueueUserWorkItem(
                    delegate { AddSynced(type, user == null ? 0 : user.Id, nodeId, comment); });
                return;
            }

            AddSynced(type, user == null ? 0 : user.Id, nodeId, comment);
        }

        /// <summary>
        /// Adds the specified log item to the log without any user information attached.
        /// </summary>
        /// <param name="type">The log type.</param>
        /// <param name="nodeId">The affected node id.</param>
        /// <param name="comment">Comment.</param>
        public static void Add(LogTypes type, int nodeId, string comment)
        {
            Add(type, null, nodeId, comment);
        }

        /// <summary>
        /// Adds a log item to the log immidiately instead of Queuing it as a work item.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="nodeId">The node id.</param>
        /// <param name="comment">The comment.</param>
        public static void AddSynced(LogTypes type, int userId, int nodeId, string comment)
        {
            try
            {
                SqlHelper.ExecuteNonQuery(
                    "insert into umbracoLog (userId, nodeId, logHeader, logComment) values (@userId, @nodeId, @logHeader, @comment)",
                    SqlHelper.CreateParameter("@userId", userId),
                    SqlHelper.CreateParameter("@nodeId", nodeId),
                    SqlHelper.CreateParameter("@logHeader", type.ToString()),
                    SqlHelper.CreateParameter("@comment", comment));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString(), "Error");
                Trace.WriteLine(e.ToString());
            }
        }

        public List<LogItem> GetAuditLogItems(int NodeId)
        {
            if (UmbracoSettings.ExternalLoggerLogAuditTrail && ExternalLogger != null)
                return ExternalLogger.GetAuditLogReader(NodeId);
            else
                return LogItem.ConvertIRecordsReader(SqlHelper.ExecuteReader(
                    "select userId, nodeId, logHeader, DateStamp, logComment from umbracoLog where nodeId = @id and logHeader not in ('open','system') order by DateStamp desc",
                    SqlHelper.CreateParameter("@id", NodeId)));
        }

        public List<LogItem> GetLogItems(LogTypes type, DateTime sinceDate)
        {
            if (ExternalLogger != null)
                return ExternalLogger.GetLogItems(type, sinceDate);
            else
                return LogItem.ConvertIRecordsReader(SqlHelper.ExecuteReader(
                "select userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where logHeader = @logHeader and DateStamp >= @dateStamp order by dateStamp desc",
                SqlHelper.CreateParameter("@logHeader", type),
                SqlHelper.CreateParameter("@dateStamp", sinceDate)));
        }

        public List<LogItem> GetLogItems(int nodeId)
        {
            if (ExternalLogger != null)
                return ExternalLogger.GetLogItems(nodeId);
            else
                return LogItem.ConvertIRecordsReader(SqlHelper.ExecuteReader(
                "select userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where id = @id order by dateStamp desc",
                SqlHelper.CreateParameter("@id", nodeId)));
        }

        public List<LogItem> GetLogItems(User user, DateTime sinceDate)
        {
            if (ExternalLogger != null)
                return ExternalLogger.GetLogItems(user, sinceDate);
            else
                return LogItem.ConvertIRecordsReader(SqlHelper.ExecuteReader(
                "select userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where UserId = @user and DateStamp >= @dateStamp order by dateStamp desc",
                SqlHelper.CreateParameter("@user", user.Id),
                SqlHelper.CreateParameter("@dateStamp", sinceDate)));
        }

        public List<LogItem> GetLogItems(User user, LogTypes type, DateTime sinceDate)
        {
            if (ExternalLogger != null)
                return ExternalLogger.GetLogItems(user, type, sinceDate);
            else
                return LogItem.ConvertIRecordsReader(SqlHelper.ExecuteReader(
                "select userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where UserId = @user and logHeader = @logHeader and DateStamp >= @dateStamp order by dateStamp desc",
                SqlHelper.CreateParameter("@logHeader", type),
                SqlHelper.CreateParameter("@user", user.Id),
                SqlHelper.CreateParameter("@dateStamp", sinceDate)));
        }

        public static void CleanLogs(int maximumAgeOfLogsInMinutes)
        {
            if (Instance.ExternalLogger != null)
                Instance.ExternalLogger.CleanLogs(maximumAgeOfLogsInMinutes);
            else
            {
                try
                {
                    DateTime oldestPermittedLogEntry = DateTime.Now.Subtract(new TimeSpan(0, maximumAgeOfLogsInMinutes, 0));
                    SqlHelper.ExecuteNonQuery("delete from umbracoLog where datestamp < @oldestPermittedLogEntry and logHeader in ('open','system')",
                        SqlHelper.CreateParameter("@oldestPermittedLogEntry", oldestPermittedLogEntry));
                    Add(LogTypes.System, -1, "Log scrubbed.  Removed all items older than " + oldestPermittedLogEntry);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString(), "Error");
                    Trace.WriteLine(e.ToString());
                }
            }
        }


        #region New GetLog methods - DataLayer layer compatible
        /// <summary>
        /// Gets a reader for the audit log.
        /// </summary>
        /// <param name="NodeId">The node id.</param>
        /// <returns>A reader for the audit log.</returns>
        [Obsolete("Use the Instance.GetAuditLogItems method which return a list of LogItems instead")]
        public static IRecordsReader GetAuditLogReader(int NodeId)
        {
            return SqlHelper.ExecuteReader(
                "select u.userName as [User], logHeader as Action, DateStamp as Date, logComment as Comment from umbracoLog inner join umbracoUser u on u.id = userId where nodeId = @id and logHeader not in ('open','system') order by DateStamp desc",
                SqlHelper.CreateParameter("@id", NodeId));
        }

        /// <summary>
        /// Gets a reader for the log for the specified types.
        /// </summary>
        /// <param name="Type">The type of log message.</param>
        /// <param name="SinceDate">The start date.</param>
        /// <returns>A reader for the log.</returns>
        [Obsolete("Use the Instance.GetLogItems method which return a list of LogItems instead")]
        public static IRecordsReader GetLogReader(LogTypes Type, DateTime SinceDate)
        {
            return SqlHelper.ExecuteReader(
                "select userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where logHeader = @logHeader and DateStamp >= @dateStamp order by dateStamp desc",
                SqlHelper.CreateParameter("@logHeader", Type.ToString()),
                SqlHelper.CreateParameter("@dateStamp", SinceDate));
        }

        /// <summary>
        /// Gets a reader for the log of the specified node.
        /// </summary>
        /// <param name="NodeId">The node id.</param>
        /// <returns>A reader for the log.</returns>
        [Obsolete("Use the Instance.GetLogItems method which return a list of LogItems instead")]
        public static IRecordsReader GetLogReader(int NodeId)
        {
            return SqlHelper.ExecuteReader(
                "select u.userName, DateStamp, logHeader, logComment from umbracoLog inner join umbracoUser u on u.id = userId where nodeId = @id",
                SqlHelper.CreateParameter("@id", NodeId));
        }

        /// <summary>
        /// Gets a reader for the log for the specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="SinceDate">The start date.</param>
        /// <returns>A reader for the log.</returns>
        [Obsolete("Use the Instance.GetLogItems method which return a list of LogItems instead")]
        public static IRecordsReader GetLogReader(User user, DateTime SinceDate)
        {
            return SqlHelper.ExecuteReader(
                "select userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where UserId = @user and DateStamp >= @dateStamp order by dateStamp desc",
                SqlHelper.CreateParameter("@user", user.Id),
                SqlHelper.CreateParameter("@dateStamp", SinceDate));
        }

        /// <summary>
        /// Gets a reader of specific for the log for specific types and a specified user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="Type">The type of log message.</param>
        /// <param name="SinceDate">The since date.</param>
        /// <returns>A reader for the log.</returns>
        [Obsolete("Use the Instance.GetLogItems method which return a list of LogItems instead")]
        public static IRecordsReader GetLogReader(User user, LogTypes Type, DateTime SinceDate)
        {
            return SqlHelper.ExecuteReader(
                "select userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where UserId = @user and logHeader = @logHeader and DateStamp >= @dateStamp order by dateStamp desc",
                SqlHelper.CreateParameter("@logHeader", Type.ToString()),
                SqlHelper.CreateParameter("@user", user.Id),
                SqlHelper.CreateParameter("@dateStamp", SinceDate));
        }
        #endregion

        #endregion
    }

    /// <summary>
    /// The collection of available log types.
    /// </summary>
    public enum LogTypes
    {
        /// <summary>
        /// Used when new nodes are added
        /// </summary>
        [AuditTrailLogItem]
        New,
        /// <summary>
        /// Used when nodes are saved
        /// </summary>
        [AuditTrailLogItem]
        Save,
        /// <summary>
        /// Used when nodes are opened
        /// </summary>
        [AuditTrailLogItem]
        Open,
        /// <summary>
        /// Used when nodes are deleted
        /// </summary>
        [AuditTrailLogItem]
        Delete,
        /// <summary>
        /// Used when nodes are published
        /// </summary>
        [AuditTrailLogItem]
        Publish,
        /// <summary>
        /// Used when nodes are send to publishing
        /// </summary>
        [AuditTrailLogItem]
        SendToPublish,
        /// <summary>
        /// Used when nodes are unpublished
        /// </summary>
        [AuditTrailLogItem]
        UnPublish,
        /// <summary>
        /// Used when nodes are moved
        /// </summary>
        [AuditTrailLogItem]
        Move,
        /// <summary>
        /// Used when nodes are copied
        /// </summary>
        [AuditTrailLogItem]
        Copy,
        /// <summary>
        /// Used when nodes are assígned a domain
        /// </summary>
        [AuditTrailLogItem]
        AssignDomain,
        /// <summary>
        /// Used when public access are changed for a node
        /// </summary>
        [AuditTrailLogItem]
        PublicAccess,
        /// <summary>
        /// Used when nodes are sorted
        /// </summary>
        [AuditTrailLogItem]
        Sort,
        /// <summary>
        /// Used when a notification are send to a user
        /// </summary>
        [AuditTrailLogItem]
        Notify,
        /// <summary>
        /// Used when a user logs into the umbraco back-end
        /// </summary>
        Login,
        /// <summary>
        /// Used when a user logs out of the umbraco back-end
        /// </summary>
        Logout,
        /// <summary>
        /// Used when a user login fails
        /// </summary>
        LoginFailure,
        /// <summary>
        /// General system notification
        /// </summary>
        [AuditTrailLogItem]
        System,
        /// <summary>
        /// System debugging notification
        /// </summary>
        Debug,
        /// <summary>
        /// System error notification
        /// </summary>
        Error,
        /// <summary>
        /// Notfound error notification
        /// </summary>
        NotFound,
        /// <summary>
        /// Used when a node's content is rolled back to a previous version
        /// </summary>
        [AuditTrailLogItem]
        RollBack,
        /// <summary>
        /// Used when a package is installed
        /// </summary>
        [AuditTrailLogItem]
        PackagerInstall,
        /// <summary>
        /// Used when a package is uninstalled
        /// </summary>
        [AuditTrailLogItem]
        PackagerUninstall,
        /// <summary>
        /// Used when a ping is send to/from the system
        /// </summary>
        Ping,
        /// <summary>
        /// Used when a node is send to translation
        /// </summary>
        [AuditTrailLogItem]
        SendToTranslate,
        /// <summary>
        /// Notification from a Scheduled task.
        /// </summary>
        ScheduledTask,
        /// <summary>
        /// Use this log action for custom log messages that should be shown in the audit trail
        /// </summary>
        [AuditTrailLogItem]
        Custom
    }

    public class LogItem
    {
        public int UserId { get; set; }
        public int NodeId { get; set; }
        public DateTime Timestamp { get; set; }
        public LogTypes LogType { get; set; }
        public string Comment { get; set; }

        public LogItem()
        {

        }

        public LogItem(int userId, int nodeId, DateTime timestamp, LogTypes logType, string comment)
        {
            UserId = userId;
            NodeId = nodeId;
            Timestamp = timestamp;
            LogType = logType;
            Comment = comment;
        }

        public static List<LogItem> ConvertIRecordsReader(IRecordsReader reader)
        {
            List<LogItem> items = new List<LogItem>();
            while (reader.Read())
            {
                items.Add(new LogItem(
                    reader.GetInt("userId"),
                    reader.GetInt("nodeId"),
                    reader.GetDateTime("DateStamp"),
                    convertLogHeader(reader.GetString("logHeader")),
                    reader.GetString("logComment")));
            }

            return items;

        }

        private static LogTypes convertLogHeader(string logHeader)
        {
            try
            {
                return (LogTypes)Enum.Parse(typeof(LogTypes), logHeader, true);
            }
            catch
            {
                return LogTypes.Custom;
            }
        }
    }

    public class AuditTrailLogItem : Attribute
    {
        public AuditTrailLogItem()
        {

        }
    }


}