/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using SH = Microsoft.ApplicationBlocks.Data.SqlHelper;
using System.Diagnostics;

namespace umbraco.DataLayer.SqlHelpers.SqlServer
{
    /// <summary>
    /// Sql Helper for an SQL Server database.
    /// </summary>
    public class SqlServerHelper : SqlHelper<SqlParameter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerHelper"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SqlServerHelper(string connectionString) : base(connectionString)
        {
            m_Utility = new SqlServerUtility(this);
        }

        /// <summary>
        /// Creates a new parameter for use with this specific implementation of ISqlHelper.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <param name="value">Value of the parameter.</param>
        /// <returns>A new parameter of the correct type.</returns>
        /// <remarks>Abstract factory pattern</remarks>
        public override IParameter CreateParameter(string parameterName, object value)
        {
            return new SqlServerParameter(parameterName, value);
        }

        /// <summary>
        /// Executes a command that returns a single value.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The return value of the command.</returns>
        protected override object ExecuteScalar(string commandText, SqlParameter[] parameters)
        {
            #if DEBUG && DebugDataLayer
                // Log Query Execution
                Trace.TraceInformation(GetType().Name + " SQL ExecuteScalar: " + commandText);
#endif

            using (var cc = UseCurrentConnection)
            {
                return cc.Transaction == null 
                    ? SH.ExecuteScalar((SqlConnection) cc.Connection, CommandType.Text, commandText, parameters) 
                    : SH.ExecuteScalar((SqlTransaction) cc.Transaction, CommandType.Text, commandText, parameters);
            }
        }

        /// <summary>
        /// Executes a command and returns the number of rows affected.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The number of rows affected by the command.
        /// </returns>
        protected override int ExecuteNonQuery(string commandText, params SqlParameter[] parameters)
        {
            #if DEBUG && DebugDataLayer
                // Log Query Execution
                Trace.TraceInformation(GetType().Name + " SQL ExecuteNonQuery: " + commandText);
#endif

            using (var cc = UseCurrentConnection)
            {
                return cc.Transaction == null
                    ? SH.ExecuteNonQuery((SqlConnection) cc.Connection, CommandType.Text, commandText, parameters)
                    : SH.ExecuteNonQuery((SqlTransaction) cc.Transaction, CommandType.Text, commandText, parameters);
            }
        }

        /// <summary>
        /// Executes a command and returns a records reader containing the results.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// A data reader containing the results of the command.
        /// </returns>
        protected override IRecordsReader ExecuteReader(string commandText, params SqlParameter[] parameters)
        {
            #if DEBUG && DebugDataLayer
                // Log Query Execution
                Trace.TraceInformation(GetType().Name + " SQL ExecuteReader: " + commandText);
#endif

            using (var cc = UseCurrentConnection)
            {
                return cc.Transaction == null
                    ? new SqlServerDataReader(SH.ExecuteReader((SqlConnection) cc.Connection, CommandType.Text, commandText, parameters))
                    : new SqlServerDataReader(SH.ExecuteReader((SqlTransaction) cc.Transaction, CommandType.Text, commandText, parameters));
            }
        }
    }
}