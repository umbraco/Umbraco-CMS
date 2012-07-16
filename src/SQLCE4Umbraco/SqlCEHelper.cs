/************************************************************************************
 * 
 *  Umbraco Data Layer
 *  MIT Licensed work
 *  ©2008 Ruben Verborgh
 * 
 ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Linq;
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
        /// Checks if the actual database exists, if it doesn't then it will create it
        /// </summary>
        internal void CreateEmptyDatabase()
        {
            var localConnection = new SqlCeConnection(ConnectionString);
            if (!System.IO.File.Exists(localConnection.Database))
            {
                var sqlCeEngine = new SqlCeEngine(ConnectionString);
                sqlCeEngine.CreateDatabase();
            }
        }

        /// <summary>
        /// Most likely only will be used for unit tests but will remove all tables from the database
        /// </summary>
        internal void ClearDatabase()
        {
            var localConnection = new SqlCeConnection(ConnectionString);
            var dbFile = localConnection.Database;
            if (System.IO.File.Exists(dbFile))
            {
                var tables = new List<string>();
                using (var reader = ExecuteReader("select table_name from information_schema.tables where TABLE_TYPE <> 'VIEW'"))
                {
                    while (reader.Read())
                    {
                        tables.Add(reader.GetString("TABLE_NAME"));
                    }
                }

                while(tables.Any())
                {
                    for (var i = 0; i < tables.Count; i++)
                    {
                        var dropTable = "DROP TABLE " + tables[i];

                        try
                        {
                            ExecuteNonQuery(dropTable);
                            tables.Remove(tables[i]);
                        }
                        catch (SqlHelperException ex)
                        {
                            //this will occur because there is no cascade option, so we just wanna try the next one       
                        }
                    }
                }                               
            }
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