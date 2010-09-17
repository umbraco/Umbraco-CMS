/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Xml;
using System.Diagnostics;
using umbraco.DataLayer;
using umbraco.DataLayer.SqlHelpers.SqlServer;


namespace SqlCE4Umbraco
{
    /// <summary>
    /// Sql Helper for an SQL Server database.
    /// </summary>
    public class SqlCEHelper : SqlHelper<SqlCeParameter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlCEHelper"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SqlCEHelper(string connectionString) : base(connectionString)
        {
            m_Utility = new SqlCEUtility(this);
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
            return new SqlCEParameter(parameterName, value);
        }

        /// <summary>
        /// Executes a command that returns a single value.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>The return value of the command.</returns>
        protected override object ExecuteScalar(string commandText, SqlCeParameter[] parameters)
        {
            #if DEBUG && DebugDataLayer
                // Log Query Execution
                Trace.TraceInformation(GetType().Name + " SQL ExecuteScalar: " + commandText);
            #endif

            return SqlCeApplicationBlock.ExecuteScalar(ConnectionString, CommandType.Text, commandText, parameters);
        }

        /// <summary>
        /// Executes a command and returns the number of rows affected.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// The number of rows affected by the command.
        /// </returns>
        protected override int ExecuteNonQuery(string commandText, SqlCeParameter[] parameters)
        {
            #if DEBUG && DebugDataLayer
                // Log Query Execution
                Trace.TraceInformation(GetType().Name + " SQL ExecuteNonQuery: " + commandText);
            #endif

            return SqlCeApplicationBlock.ExecuteNonQuery(ConnectionString, CommandType.Text, commandText, parameters);
        }

        /// <summary>
        /// Executes a command and returns a records reader containing the results.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>
        /// A data reader containing the results of the command.
        /// </returns>
        protected override IRecordsReader ExecuteReader(string commandText, SqlCeParameter[] parameters)
        {
            #if DEBUG && DebugDataLayer
                // Log Query Execution
                Trace.TraceInformation(GetType().Name + " SQL ExecuteReader: " + commandText);
            #endif

            return new SqlCeDataReaderHelper(SqlCeApplicationBlock.ExecuteReader(ConnectionString, CommandType.Text,
                                                            commandText, parameters));
        }
    }
}