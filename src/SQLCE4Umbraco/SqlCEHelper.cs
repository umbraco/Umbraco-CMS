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
            if (!System.IO.File.Exists(ReplaceDataDirectory(localConnection.Database)))
            {
                var sqlCeEngine = new SqlCeEngine(ConnectionString);
                sqlCeEngine.CreateDatabase();

                // SD: Pretty sure this should be in a using clause but i don't want to cause unknown side-effects here
                // since it's been like this for quite some time
                //using (var sqlCeEngine = new SqlCeEngine(ConnectionString))
                //{
                //    sqlCeEngine.CreateDatabase();    
                //}
            }
        }

        /// <summary>
        /// Most likely only will be used for unit tests but will remove all tables from the database
        /// </summary>
        internal void ClearDatabase()
        {
            // drop constraints before tables to avoid exceptions
            // looping on try/catching exceptions was not really nice

            // http://stackoverflow.com/questions/536350/drop-all-the-tables-stored-procedures-triggers-constriants-and-all-the-depend

            var localConnection = new SqlCeConnection(ConnectionString);
            if (System.IO.File.Exists(ReplaceDataDirectory(localConnection.Database)))
            {
                List<string> tables;

                // drop foreign keys
                // SQL may need "where constraint_catalog=DB_NAME() and ..."
                tables = new List<string>();
                using (var reader = ExecuteReader("select table_name from information_schema.table_constraints where constraint_type = 'FOREIGN KEY' order by table_name"))
                {
                    while (reader.Read()) tables.Add(reader.GetString("table_name").Trim());
                }

                foreach (var table in tables)
                {
                    var constraints = new List<string>();
                    using (var reader = ExecuteReader("select constraint_name from information_schema.table_constraints where constraint_type = 'FOREIGN KEY' and table_name = '" + table + "' order by constraint_name"))
                    {
                        while (reader.Read()) constraints.Add(reader.GetString("constraint_name").Trim());
                    }
                    foreach (var constraint in constraints)
                    {
                        // SQL may need "[dbo].[table]"
                        ExecuteNonQuery("alter table [" + table + "] drop constraint [" + constraint + "]");
                    }
                }

                // drop primary keys
                // SQL may need "where constraint_catalog=DB_NAME() and ..."
                tables = new List<string>();
                using (var reader = ExecuteReader("select table_name from information_schema.table_constraints where constraint_type = 'PRIMARY KEY' order by table_name"))
                {
                    while (reader.Read()) tables.Add(reader.GetString("table_name").Trim());
                }

                foreach (var table in tables)
                {
                    var constraints = new List<string>();
                    using (var reader = ExecuteReader("select constraint_name from information_schema.table_constraints where constraint_type = 'PRIMARY KEY' and table_name = '" + table + "' order by constraint_name"))
                    {
                        while (reader.Read()) constraints.Add(reader.GetString("constraint_name").Trim());
                    }
                    foreach (var constraint in constraints)
                    {
                        // SQL may need "[dbo].[table]"
                        ExecuteNonQuery("alter table [" + table + "] drop constraint [" + constraint + "]");
                    }
                }

                // drop tables
                tables = new List<string>();
                using (var reader = ExecuteReader("select table_name from information_schema.tables where table_type <> 'VIEW' order by table_name"))
                {
                    while (reader.Read()) tables.Add(reader.GetString("table_name").Trim());
                }

                foreach (var table in tables)
                {
                    ExecuteNonQuery("drop table [" + table + "]");
                }
            }
        }

        /// <summary>
        /// Drops all foreign keys on a table.
        /// </summary>
        /// <param name="table">The name of the table.</param>
        /// <remarks>To be used in unit tests.</remarks>
        internal void DropForeignKeys(string table)
        {
            var constraints = new List<string>();
            using (var reader = ExecuteReader("select constraint_name from information_schema.table_constraints where constraint_type = 'FOREIGN KEY' and table_name = '" + table + "' order by constraint_name"))
            {
                while (reader.Read()) constraints.Add(reader.GetString("constraint_name").Trim());
            }
            foreach (var constraint in constraints)
            {
                // SQL may need "[dbo].[table]"
                ExecuteNonQuery("alter table [" + table + "] drop constraint [" + constraint + "]");
            }
        }

        /// <summary>
        /// Replaces the data directory with a local path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A local path with the resolved 'DataDirectory' mapping.</returns>
        private string ReplaceDataDirectory(string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && path.Contains("|DataDirectory|"))
            {
                var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                if (!string.IsNullOrEmpty(dataDirectory))
                {
                    path = path.Contains(@"|\") 
                        ? path.Replace("|DataDirectory|", dataDirectory) 
                        : path.Replace("|DataDirectory|", dataDirectory + System.IO.Path.DirectorySeparatorChar);
                }
            }

            return path;
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


        internal IRecordsReader ExecuteReader(string commandText)
        {
            return ExecuteReader(commandText, new SqlCEParameter(string.Empty, string.Empty));
        }


        internal int ExecuteNonQuery(string commandText)
        {
            return ExecuteNonQuery(commandText, new SqlCEParameter(string.Empty, string.Empty));
        }
    }
}