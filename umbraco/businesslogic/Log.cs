using System;
using System.Data;
using System.Diagnostics;
using System.Threading;

using umbraco.DataLayer;

namespace umbraco.BusinessLogic
{
	/// <summary>
	/// Summary description for Log.
	/// </summary>
	public class Log
	{
		#region Statics

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
			if(!UmbracoSettings.EnableLogging) return;

            if (UmbracoSettings.DisabledLogTypes != null &&
                UmbracoSettings.DisabledLogTypes.SelectSingleNode(String.Format("//logTypeAlias [. = '{0}']", type.ToString().ToLower())) == null) {

                if (comment.Length > 3999)
                    comment = comment.Substring(0, 3955) + "...";

                if (UmbracoSettings.EnableAsyncLogging) {
                    ThreadPool.QueueUserWorkItem(
                        delegate { AddSynced(type, user == null ? 0 : user.Id, nodeId, comment); });
                    return;
                }

                AddSynced(type, user == null ? 0 : user.Id, nodeId, comment);
            }
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
			catch(Exception e)
			{
				Debug.WriteLine(e.ToString(), "Error");
				Trace.WriteLine(e.ToString());
			}
		}

        #region New GetLog methods - DataLayer layer compatible
        /// <summary>
        /// Gets a reader for the audit log.
        /// </summary>
        /// <param name="NodeId">The node id.</param>
        /// <returns>A reader for the audit log.</returns>
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
        public static IRecordsReader GetLogReader(User user, LogTypes Type, DateTime SinceDate)
        {
            return SqlHelper.ExecuteReader(
                "select userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where UserId = @user and logHeader = @logHeader and DateStamp >= @dateStamp order by dateStamp desc",
                SqlHelper.CreateParameter("@logHeader", Type.ToString()),
                SqlHelper.CreateParameter("@user", user.Id),
                SqlHelper.CreateParameter("@dateStamp", SinceDate));
        } 
        #endregion

        #region Old GetLog methods - DataLayer incompatible
        #pragma warning disable 618 // ConvertToDataSet is obsolete

        /// <summary>
        /// Gets the log.
        /// </summary>
        /// <param name="Type">The type.</param>
        /// <param name="SinceDate">The start date.</param>
        /// <returns>The log.</returns>
        /// <remarks>Only guaranteed to work with SQL Server. Obsolete.</remarks>
        [Obsolete("Not compatible with the data layer. Use GetLogReader instead.", true)]
        public static DataSet GetLog(LogTypes Type, DateTime SinceDate)
        {
            try
            {
                return ConvertToDataSet(GetLogReader(Type, SinceDate));
            }
            catch (Exception)
            {
                throw new Exception("The GetLog method is not compatible with the data layer.");
            }
        }

        /// <summary>
        /// Returns a dataset of Log items with a specific type since a specific date.
        /// </summary>
        /// <param name="Type">The type.</param>
        /// <param name="SinceDate">The start date.</param>
        /// <param name="Limit">Maximum number of results.</param>
        /// <returns>The log.</returns>
        /// <remarks>Only guaranteed to work with SQL Server. Obsolete.</remarks>
        [Obsolete("Not compatible with the data layer. Use GetLogReader instead.", true)]
        public static DataSet GetLog(LogTypes Type, DateTime SinceDate, int Limit)
        {
            try
            {
                return ConvertToDataSet(SqlHelper.ExecuteReader(
                    "select top " + Limit +
                    " userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where logHeader = @logHeader and DateStamp >= @dateStamp order by dateStamp desc",
                    SqlHelper.CreateParameter("@logHeader", Type.ToString()),
                    SqlHelper.CreateParameter("@dateStamp", SinceDate)));
            }
            catch (Exception)
            {
                throw new Exception("The GetLog method is not compatible with the data layer.");
            }
        }

        /// <summary>
        /// Returns a dataset of Log items for a specific node
        /// </summary>
        /// <param name="NodeId">The node id.</param>
        /// <returns>The log.</returns>
        /// <remarks>Only guaranteed to work with SQL Server. Obsolete.</remarks>
        [Obsolete("Not compatible with the data layer. Use GetLogReader instead.", true)]
        public static DataSet GetLog(int NodeId)
        {
            try
            {
                return ConvertToDataSet(GetLogReader(NodeId));
            }
            catch (Exception)
            {
                throw new Exception("The GetLog method is not compatible with the data layer.");
            }
        }

        /// <summary>
        /// Returns a dataset of audit Log items for a specific node with more detailed user information
        /// </summary>
        /// <param name="NodeId">The node id.</param>
        /// <returns>The log.</returns>
        /// <remarks>Only guaranteed to work with SQL Server. Obsolete.</remarks>
        [Obsolete("Not compatible with the data layer. Use GetAuditLogReader instead.", true)]
        public static DataSet GetAuditLog(int NodeId)
        {
            try
            {
                return ConvertToDataSet(GetAuditLogReader(NodeId));
            }
            catch (Exception)
            {
                throw new Exception("The GetAuditLog method is not compatible with the data layer.");
            }
        }

        /// <summary>
        /// Returns a dataset of Log items for a specific user, since a specific date
        /// </summary>
        /// <param name="u">The user.</param>
        /// <param name="SinceDate">The start date.</param>
        /// <returns>The log.</returns>
        /// <remarks>Only guaranteed to work with SQL Server. Obsolete.</remarks>
        [Obsolete("Not compatible with the data layer. Use GetAuditLogReader instead.", true)]
        public static DataSet GetLog(User u, DateTime SinceDate)
        {
            try
            {
                return ConvertToDataSet(GetLogReader(u, SinceDate));
            }
            catch (Exception)
            {
                throw new Exception("The GetLog method is not compatible with the data layer.");
            }
        }

        /// <summary>
        /// Returns a dataset of Log items for a specific user of a specific type, since a specific date
        /// </summary>
        /// <param name="u">The user.</param>
        /// <param name="Type">The type.</param>
        /// <param name="SinceDate">The start date.</param>
        /// <returns>The log.</returns>
        /// <remarks>Only guaranteed to work with SQL Server. Obsolete.</remarks>
        [Obsolete("Not compatible with the data layer. Use GetLogReader instead.", true)]
        public static DataSet GetLog(User u, LogTypes Type, DateTime SinceDate)
        {
            try
            {
                return ConvertToDataSet(GetLogReader(u, Type, SinceDate));
            }
            catch (Exception)
            {
                throw new Exception("The GetLog method is not compatible with the data layer.");
            }
        }

        /// <summary>
        /// Returns a dataset of Log items for a specific user of a specific type, since a specific date
        /// </summary>
        /// <param name="u">The user.</param>
        /// <param name="Type">The type.</param>
        /// <param name="SinceDate">The since date.</param>
        /// <param name="Limit">The limit.</param>
        /// <returns>The log.</returns>
        /// <remarks>Only guaranteed to work with SQL Server. Obsolete.</remarks>
        [Obsolete("Not compatible with the data layer. Use GetLogReader instead.", true)]
        public static DataSet GetLog(User u, LogTypes Type, DateTime SinceDate, int Limit)
        {
            try
            {
                return ConvertToDataSet(SqlHelper.ExecuteReader(
                    "select top " + Limit +
                    " userId, NodeId, DateStamp, logHeader, logComment from umbracoLog where UserId = @user and logHeader = @logHeader and DateStamp >= @dateStamp order by dateStamp desc",
                    SqlHelper.CreateParameter("@logHeader", Type.ToString()),
                    SqlHelper.CreateParameter("@user", u.Id),
                    SqlHelper.CreateParameter("@dateStamp", SinceDate)));
            }
            catch (Exception)
            {
                throw new Exception("The GetLog method is not compatible with the data layer.");
            }
        }

        /// <summary>
        /// Converts a records reader to a data set.
        /// </summary>
        /// <param name="recordsReader">The records reader.</param>
        /// <returns>The data set</returns>
        /// <remarks>Only works with DataLayer.SqlHelpers.SqlServer.SqlServerDataReader.</remarks>
        /// <exception>When not an DataLayer.SqlHelpers.SqlServer.SqlServerDataReader.</exception>
        [Obsolete("Temporary workaround for old GetLog methods.", false)]
        private static DataSet ConvertToDataSet(IRecordsReader recordsReader)
        {
            // Get the internal RawDataReader (obsolete)
            System.Data.SqlClient.SqlDataReader reader
                = ((DataLayer.SqlHelpers.SqlServer.SqlServerDataReader)recordsReader).RawDataReader;
            DataSet dataSet = new DataSet();
            do
            {
                DataTable dataTable = new DataTable();
                DataTable schemaTable = reader.GetSchemaTable();

                if (schemaTable != null)
                {
                    // A query returning records was executed
                    foreach (DataRow dataRow in schemaTable.Rows)
                    {
                        // Create a column name that is unique in the data table
                        string columnName = (string)dataRow["ColumnName"];
                        // Add the column definition to the data table
                        DataColumn column = new DataColumn(columnName, (Type)dataRow["DataType"]);
                        dataTable.Columns.Add(column);
                    }

                    dataSet.Tables.Add(dataTable);

                    // Fill the data table we just created
                    while (reader.Read())
                    {
                        DataRow dataRow = dataTable.NewRow();
                        for (int i = 0; i < reader.FieldCount; i++)
                            dataRow[i] = reader.GetValue(i);
                        dataTable.Rows.Add(dataRow);
                    }
                }
                else
                {
                    // No records were returned, return number of rows affected
                    dataTable.Columns.Add(new DataColumn("RowsAffected"));
                    dataSet.Tables.Add(dataTable);
                    DataRow rowsAffectedRow = dataTable.NewRow();
                    rowsAffectedRow[0] = reader.RecordsAffected;
                    dataTable.Rows.Add(rowsAffectedRow);
                }
            }
            // Go trough all result sets
            while (reader.NextResult());

            // Close the data reader so the underlying connection is closed
            recordsReader.Close();

            return dataSet;
        }
        #pragma warning restore 618

        #endregion

        public static void CleanLogs(int maximumAgeOfLogsInMinutes)
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
		New,
        /// <summary>
        /// Used when nodes are saved
        /// </summary>
		Save,
        /// <summary>
        /// Used when nodes are opened
        /// </summary>
		Open,
        /// <summary>
        /// Used when nodes are deleted
        /// </summary>
		Delete,
        /// <summary>
        /// Used when nodes are published
        /// </summary>
		Publish,
        /// <summary>
        /// Used when nodes are send to publishing
        /// </summary>
		SendToPublish,
        /// <summary>
        /// Used when nodes are unpublished
        /// </summary>
		UnPublish,
        /// <summary>
        /// Used when nodes are moved
        /// </summary>
		Move,
        /// <summary>
        /// Used when nodes are copied
        /// </summary>
		Copy,
        /// <summary>
        /// Used when nodes are assígned a domain
        /// </summary>
		AssignDomain,
        /// <summary>
        /// Used when public access are changed for a node
        /// </summary>
		PublicAccess,
        /// <summary>
        /// Used when nodes are sorted
        /// </summary>
		Sort,
        /// <summary>
        /// Used when a notification are send to a user
        /// </summary>
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
		RollBack,
        /// <summary>
        /// Used when a package is installed
        /// </summary>
		PackagerInstall,
        /// <summary>
        /// Used when a package is uninstalled
        /// </summary>
		PackagerUninstall,
        /// <summary>
        /// Used when a ping is send to/from the system
        /// </summary>
		Ping,
        /// <summary>
        /// Used when a node is send to translation
        /// </summary>
		SendToTranslate,
        /// <summary>
        /// Notification from a Scheduled task.
        /// </summary>
		ScheduledTask,
        /// <summary>
        /// Use this log action for custom log messages that should be shown in the audit trail
        /// </summary>
        Custom
	}
}