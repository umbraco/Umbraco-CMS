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
            }
        }

        //SD: Commented this out, it was used to remove all data in the database but not the database schema....
        // hoping for faster unit test performance, unfortunately I don't think it is currently possible to 
        // install the base umbraco data without dropping constraints. For some reason I don't think the performance
        // will be much faster dropping and re-creating constraints as opposed to just re-creating the schema.
//        internal void ClearUmbracoDatabaseData()
//        {
//            var tables = new List<string>();
//            using (var reader = ExecuteReader("select table_name from information_schema.tables where table_type <> 'VIEW' order by table_name"))            
//            {
//                while (reader.Read()) tables.Add(reader.GetString("table_name").Trim());
//            }

//            var pKeys = new List<Tuple<string, string>>();
//            //first get the primary key columsn for each table:
//            using (var reader = ExecuteReader(@"
//select information_schema.table_constraints.table_name, column_name from information_schema.table_constraints
//inner join information_schema.key_column_usage
//on information_schema.table_constraints.constraint_name = information_schema.key_column_usage.constraint_name
//where constraint_type = 'PRIMARY KEY'"))
//            {
//                while (reader.Read())
//                {
//                    pKeys.Add(new Tuple<string, string>(reader.GetString("table_name").Trim(), reader.GetString("column_name").Trim()));
//                }
//            }

//            //exclude the umbracoNode table, it is special and has a recursive constraing so we'll deal with that last
//            tables = tables.Where(x => !x.InvariantEquals("umbracoNode")).ToList();

//            var retries = -1;

//            //Clear all data in all tables except for umbracoNode... this will always be last
//            while (tables.Any())
//            {
//                retries++;

//                //avoid an infinite loop if there's something seriously wrong
//                if (retries > 100)
//                {
//                    throw new ApplicationException("Could not clear out all of the data in the database :(");
//                }

//                var currTables = tables.ToArray();
//                for (int index = 0; index < currTables.Count(); index++)
//                {
//                    var table = currTables[index];
//                    try
//                    {

//                        //get all foreign key constraint columns for the table
//                        var fkCols = new List<string>();
//                        using (var reader = ExecuteReader(@"
//select column_name
//  from information_schema.key_column_usage
//  inner join information_schema.table_constraints on
//  information_schema.key_column_usage.constraint_name = 
//  information_schema.table_constraints.constraint_name
//where constraint_type = 'FOREIGN KEY' AND information_schema.key_column_usage.table_name = '" + table + "'"))
//                        {
//                            while (reader.Read()) fkCols.Add(reader.GetString("column_name").Trim());
//                        }

//                        //set the value to null for everything in these columns (we do a try catch in case null is not allowed)
//                        foreach (var fk in fkCols)
//                        {
//                            try
//                            {
//                                ExecuteNonQuery("UPDATE " + table + " SET " + fk + " = NULL");
//                            }
//                            catch (Exception)
//                            {
//                                //swallow this exception and continue, apparently null is not allowed here
//                            }
//                        }

//                        //SINCE SqlCe doesn't support "truncate table" we need to do this and reset the identity seed
//                        ExecuteNonQuery("delete from [" + table + "]");
//                        foreach (var key in pKeys.Where(x => x.Item1.InvariantEquals(table)))
//                        {
//                            try
//                            {
//                                ExecuteNonQuery("ALTER TABLE [" + table + "] ALTER COLUMN " + key.Item2 + " IDENTITY (1,1)");
//                            }
//                            catch (Exception)
//                            {
//                                //swallow... SD: we're swallowing this in case the key doesn't have an identity, there might be a better way to 
//                                // look this up but I can't find it.
//                            }   
//                        }                        
//                        tables.Remove(table); //table is complete
//                    }
//                    catch (Exception)
//                    {
//                        //swallow... SD: I know this isn't  nice but it's just as fast and trying to identify the upmost 'parent' table
//                        // and then follow its chain down to it's bottommost 'child' table based on constraints/relations is something
//                        // I don't feel like writing.... could figure it out manually but don't have time atm.
//                    }
//                }
//            }

//            //Now, we deal with umbracoNode
//            //Set all parentId's = the first item found
//            var firstId = ExecuteScalar<int>("Select TOP 1 id from umbracoNode");
//            ExecuteNonQuery("UPDATE umbracoNode SET parentId = " + firstId);
//            //now clear out the data, aparet from the foreign key we've updated too
//            ExecuteNonQuery("delete from umbracoNode where id <> " + firstId);
//            //finally clear out the last one
//            ExecuteNonQuery("delete from umbracoNode");

//        }

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
    }
}