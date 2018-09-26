﻿/* PetaPoco v4.0.3 - A Tiny ORMish thing for your POCO's.
 * Copyright © 2011 Topten Software.  All Rights Reserved.
 *
 * Apache License 2.0 - http://www.toptensoftware.com/petapoco/license
 *
 * Special thanks to Rob Conery (@robconery) for original inspiration (ie:Massive) and for
 * use of Subsonic's T4 templates, Rob Sullivan (@DataChomp) for hard core DBA advice
 * and Adam Schroder (@schotime) for lots of suggestions, improvements and Oracle support
 */

// Define PETAPOCO_NO_DYNAMIC in your project settings on .NET 3.5

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Configuration;
using System.Data.Common;
using System.Data;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Expressions;

namespace Umbraco.Core.Persistence
{
	// Poco's marked [Explicit] require all column properties to be marked
	[AttributeUsage(AttributeTargets.Class)]
	public class ExplicitColumnsAttribute : Attribute
	{
	}
	// For non-explicit pocos, causes a property to be ignored
	[AttributeUsage(AttributeTargets.Property)]
	public class IgnoreAttribute : Attribute
	{
	}

	// For explicit pocos, marks property as a column and optionally supplies column name
	[AttributeUsage(AttributeTargets.Property)]
	public class ColumnAttribute : Attribute
	{
		public ColumnAttribute() { }
		public ColumnAttribute(string name) { Name = name; }
		public string Name { get; set; }
	}

	// For explicit pocos, marks property as a result column and optionally supplies column name
	[AttributeUsage(AttributeTargets.Property)]
	public class ResultColumnAttribute : ColumnAttribute
	{
		public ResultColumnAttribute() { }
		public ResultColumnAttribute(string name) : base(name) { }
	}

	// Specify the table name of a poco
	[AttributeUsage(AttributeTargets.Class)]
	public class TableNameAttribute : Attribute
	{
		public TableNameAttribute(string tableName)
		{
			Value = tableName;
		}
		public string Value { get; private set; }
	}

	// Specific the primary key of a poco class (and optional sequence name for Oracle)
	[AttributeUsage(AttributeTargets.Class)]
	public class PrimaryKeyAttribute : Attribute
	{
		public PrimaryKeyAttribute(string primaryKey)
		{
			Value = primaryKey;
			autoIncrement = true;
		}

		public string Value { get; private set; }
		public string sequenceName { get; set; }
		public bool autoIncrement { get; set; }
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class AutoJoinAttribute : Attribute
	{
		public AutoJoinAttribute() { }
	}

	// Results from paged request
	public class Page<T>
	{
		public long CurrentPage { get; set; }
		public long TotalPages { get; set; }
		public long TotalItems { get; set; }
		public long ItemsPerPage { get; set; }
		public List<T> Items { get; set; }
		public object Context { get; set; }
	}

	// Pass as parameter value to force to DBType.AnsiString
	public class AnsiString
	{
		public AnsiString(string str)
		{
			Value = str;
		}
		public string Value { get; private set; }
	}

	// Used by IMapper to override table bindings for an object
	public class TableInfo
	{
		public string TableName { get; set; }
		public string PrimaryKey { get; set; }
		public bool AutoIncrement { get; set; }
		public string SequenceName { get; set; }
	}

	// Optionally provide an implementation of this to Database.Mapper
	public interface IMapper
	{
		void GetTableInfo(Type t, TableInfo ti);
        bool MapPropertyToColumn(Type t, PropertyInfo pi, ref string columnName, ref bool resultColumn);
		Func<object, object> GetFromDbConverter(PropertyInfo pi, Type SourceType);
		Func<object, object> GetToDbConverter(Type SourceType);
	}

	// This will be merged with IMapper in the next major version
	public interface IMapper2 : IMapper
	{
		Func<object, object> GetFromDbConverter(Type DestType, Type SourceType);
	}

	// Database class ... this is where most of the action happens
	public class Database : IDisposable
	{
		public Database(IDbConnection connection)
		{
			_sharedConnection = connection;
			_connectionString = connection.ConnectionString;
			_sharedConnectionDepth = 2;		// Prevent closing external connection
			CommonConstruct();
		}

		public Database(string connectionString, string providerName)
		{
			_connectionString = connectionString;
			_providerName = providerName;
			CommonConstruct();
		}

		public Database(string connectionString, DbProviderFactory provider)
		{
			_connectionString = connectionString;
			_factory = provider;
			CommonConstruct();
		}

		public Database(string connectionStringName)
		{
			// Use first?
			if (connectionStringName == "")
				connectionStringName = ConfigurationManager.ConnectionStrings[0].Name;

			// Work out connection string and provider name
			var providerName = Constants.DatabaseProviders.SqlServer;
			if (ConfigurationManager.ConnectionStrings[connectionStringName] != null)
			{
				if (string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings[connectionStringName].ProviderName) == false)
					providerName = ConfigurationManager.ConnectionStrings[connectionStringName].ProviderName;
			}
			else
			{
				throw new NullReferenceException("Can't find a connection string with the name '" + connectionStringName + "'");
			}

			// Store factory and connection string
			_connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
			_providerName = providerName;
			CommonConstruct();
		}

		public enum DBType
		{
			SqlServer,
			SqlServerCE,
			MySql,
			PostgreSQL,
			Oracle,
            SQLite
		}
		private DBType _dbType = DBType.SqlServer;

        public DBType DatabaseType { get { return _dbType; } }

		// Common initialization
		private void CommonConstruct()
		{
			_transactionDepth = 0;
			EnableAutoSelect = true;
			EnableNamedParams = true;
			ForceDateTimesToUtc = true;

            if (_providerName != null)
				_factory = DbProviderFactories.GetFactory(_providerName);

            string dbtype = (_factory == null ? _sharedConnection.GetType() : _factory.GetType()).Name;

			if (dbtype.StartsWith("MySql")) _dbType = DBType.MySql;
            else if (dbtype.StartsWith("SqlCe")) _dbType = DBType.SqlServerCE;
			else if (dbtype.StartsWith("Npgsql")) _dbType = DBType.PostgreSQL;
			else if (dbtype.StartsWith("Oracle")) _dbType = DBType.Oracle;
            else if (dbtype.StartsWith("SQLite")) _dbType = DBType.SQLite;

			if (_dbType == DBType.MySql && _connectionString != null && _connectionString.IndexOf("Allow User Variables=true") >= 0)
				_paramPrefix = "?";
			if (_dbType == DBType.Oracle)
				_paramPrefix = ":";

            // by default use MSSQL default ReadCommitted level
            //TODO change to RepeatableRead - but that is breaking
		    _isolationLevel = IsolationLevel.ReadCommitted;
		}

		// Automatically close one open shared connection
		public void Dispose()
		{
		    Dispose(true);
		}

	    protected virtual void Dispose(bool disposing)
	    {
            // Automatically close one open connection reference
            //  (Works with KeepConnectionAlive and manually opening a shared connection)
            CloseSharedConnection();
        }

        // Set to true to keep the first opened connection alive until this object is disposed
        public bool KeepConnectionAlive { get; set; }

		// Open a connection (can be nested)
		public void OpenSharedConnection()
		{
			if (_sharedConnectionDepth == 0)
			{
				_sharedConnection = _factory.CreateConnection();
				_sharedConnection.ConnectionString = _connectionString;
                _sharedConnection.OpenWithRetry();//Changed .Open() => .OpenWithRetry() extension method

                // ensure we have the proper isolation level, as levels can leak in pools
                // read http://stackoverflow.com/questions/9851415/sql-server-isolation-level-leaks-across-pooled-connections
                // and http://stackoverflow.com/questions/641120/what-exec-sp-reset-connection-shown-in-sql-profiler-means
                //
                // NPoco has that code in OpenSharedConnectionImp (commented out?)
                //using (var cmd = _sharedConnection.CreateCommand())
                //{
                //    cmd.CommandText = GetSqlForTransactionLevel(_isolationLevel);
                //    cmd.CommandTimeout = CommandTimeout;
                //    cmd.ExecuteNonQuery();
                //}
                //
                // although MSDN documentation for SQL CE clearly states that the above method
                // should work, it fails & reports an error parsing the query on 'TRANSACTION',
                // and Google is no help (others have faced the same issue... no solution). So,
                // switching to another method that does work on all databases.
                var tr = _sharedConnection.BeginTransaction(_isolationLevel);
                tr.Commit();
                tr.Dispose();

                _sharedConnection = OnConnectionOpened(_sharedConnection);

				if (KeepConnectionAlive)
					_sharedConnectionDepth++;		// Make sure you call Dispose
			}
			_sharedConnectionDepth++;
		}

		// Close a previously opened connection
		public void CloseSharedConnection()
		{
			if (_sharedConnectionDepth > 0)
			{
				_sharedConnectionDepth--;
				if (_sharedConnectionDepth == 0)
				{
					OnConnectionClosing(_sharedConnection);
					_sharedConnection.Dispose();
					_sharedConnection = null;
				}
			}
		}

		// Access to our shared connection
		public IDbConnection Connection
		{
			get { return _sharedConnection; }
		}

		// Helper to create a transaction scope
		public Transaction GetTransaction()
		{
            return GetTransaction(_isolationLevel);
        }

        public Transaction GetTransaction(IsolationLevel isolationLevel)
        {
            return new Transaction(this, isolationLevel);
        }

        public IsolationLevel CurrentTransactionIsolationLevel
        {
            get { return _transaction == null ? IsolationLevel.Unspecified : _transaction.IsolationLevel; }
        }

        // Use by derived repo generated by T4 templates
		public virtual void OnBeginTransaction() { }
		public virtual void OnEndTransaction() { }

		// Start a new transaction, can be nested, every call must be
		//	matched by a call to AbortTransaction or CompleteTransaction
		// Use `using (var scope=db.Transaction) { scope.Complete(); }` to ensure correct semantics
		public void BeginTransaction()
		{
            BeginTransaction(_isolationLevel);
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            _transactionDepth++;

			if (_transactionDepth == 1)
			{
				OpenSharedConnection();
			    try
			    {
			        _transaction = _sharedConnection.BeginTransaction(isolationLevel);
			    }

			    catch (Exception)
			    {
			        throw;
			    }
                _transactionCancelled = false;
				OnBeginTransaction();
			}
            else if (isolationLevel > _transaction.IsolationLevel)
                throw new Exception("Already in a transaction with a lower isolation level.");
        }

		// Internal helper to cleanup transaction stuff
		void CleanupTransaction()
		{
			OnEndTransaction();

			if (_transactionCancelled)
				_transaction.Rollback();
			else
				_transaction.Commit();

			_transaction.Dispose();
			_transaction = null;

			CloseSharedConnection();
		}

		// Abort the entire outer most transaction scope
		public void AbortTransaction()
		{
			_transactionCancelled = true;
            //TODO what shall we do if transactionDepth is already zero?
			if ((--_transactionDepth) == 0)
				CleanupTransaction();
		}

		// Complete the transaction
		public void CompleteTransaction()
		{
            //TODO what shall we do if transactionDepth is already zero?
            if ((--_transactionDepth) == 0)
				CleanupTransaction();
		}

        // in NPoco this is in DatabaseType
        private static string GetSqlForTransactionLevel(IsolationLevel isolationLevel)
        {
            switch (isolationLevel)
            {
                case IsolationLevel.ReadCommitted:
                    return "SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
                case IsolationLevel.ReadUncommitted:
                    return "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED";
                case IsolationLevel.RepeatableRead:
                    return "SET TRANSACTION ISOLATION LEVEL REPEATABLE READ";
                case IsolationLevel.Serializable:
                    return "SET TRANSACTION ISOLATION LEVEL SERIALIZABLE";
                case IsolationLevel.Snapshot:
                    return "SET TRANSACTION ISOLATION LEVEL SNAPSHOT";
                default:
                    return "SET TRANSACTION ISOLATION LEVEL READ COMMITTED";
            }
        }

        // Helper to handle named parameters from object properties
		static readonly Regex rxParams = new Regex(@"(?<!@)@\w+", RegexOptions.Compiled);
		public static string ProcessParams(string _sql, object[] args_src, List<object> args_dest)
		{
			return rxParams.Replace(_sql, m =>
			{
				string param = m.Value.Substring(1);

				object arg_val;

				int paramIndex;
				if (int.TryParse(param, out paramIndex))
				{
					// Numbered parameter
					if (paramIndex < 0 || paramIndex >= args_src.Length)
						throw new ArgumentOutOfRangeException(string.Format("Parameter '@{0}' specified but only {1} parameters supplied (in `{2}`)", paramIndex, args_src.Length, _sql));
					arg_val = args_src[paramIndex];
				}
				else
				{
					// Look for a property on one of the arguments with this name
					bool found = false;
					arg_val = null;
					foreach (var o in args_src)
					{
						var pi = o.GetType().GetProperty(param);
						if (pi != null)
						{
							arg_val = pi.GetValue(o, null);
							found = true;
							break;
						}
					}

					if (!found)
						throw new ArgumentException(string.Format("Parameter '@{0}' specified but none of the passed arguments have a property with this name (in '{1}')", param, _sql));
				}

				// Expand collections to parameter lists
				if ((arg_val as System.Collections.IEnumerable) != null &&
					(arg_val as string) == null &&
					(arg_val as byte[]) == null)
				{
					var sb = new StringBuilder();
					foreach (var i in arg_val as System.Collections.IEnumerable)
					{
						sb.Append((sb.Length == 0 ? "@" : ",@") + args_dest.Count.ToString());
						args_dest.Add(i);
					}
					return sb.ToString();
				}
				else
				{
					args_dest.Add(arg_val);
					return "@" + (args_dest.Count - 1).ToString();
				}
			}
			);
		}

		// Add a parameter to a DB command
		internal void AddParam(IDbCommand cmd, object item, string ParameterPrefix)
		{
			// Convert value to from poco type to db type
			if (Database.Mapper != null && item!=null)
			{
				var fn = Database.Mapper.GetToDbConverter(item.GetType());
				if (fn!=null)
					item = fn(item);
			}

			// Support passed in parameters
			var idbParam = item as IDbDataParameter;
			if (idbParam != null)
			{
				idbParam.ParameterName = string.Format("{0}{1}", ParameterPrefix, cmd.Parameters.Count);
				cmd.Parameters.Add(idbParam);
				return;
			}

			var p = cmd.CreateParameter();
			p.ParameterName = string.Format("{0}{1}", ParameterPrefix, cmd.Parameters.Count);
			if (item == null)
			{
				p.Value = DBNull.Value;
			}
			else
			{
				var t = item.GetType();
				if (t.IsEnum)		// PostgreSQL .NET driver wont cast enum to int
				{
					p.Value = (int)item;
				}
				else if (t == typeof(Guid))
				{
					p.Value = item.ToString();
					p.DbType = DbType.String;
					p.Size = 40;
				}
				else if (t == typeof(string))
				{
                    // out of memory exception occurs if trying to save more than 4000 characters to SQL Server CE NText column.
                    //Set before attempting to set Size, or Size will always max out at 4000
                    if ((item as string).Length + 1 > 4000 && p.GetType().Name == "SqlCeParameter")
                        p.GetType().GetProperty("SqlDbType").SetValue(p, SqlDbType.NText, null);

                    p.Size = (item as string).Length + 1;
                    if(p.Size < 4000)
                        p.Size = Math.Max((item as string).Length + 1, 4000);		// Help query plan caching by using common size

					p.Value = item;
				}
				else if (t == typeof(AnsiString))
				{
					// Thanks @DataChomp for pointing out the SQL Server indexing performance hit of using wrong string type on varchar
					p.Size = Math.Max((item as AnsiString).Value.Length + 1, 4000);
					p.Value = (item as AnsiString).Value;
					p.DbType = DbType.AnsiString;
				}
				else if (t == typeof(bool) && _dbType != DBType.PostgreSQL)
				{
					p.Value = ((bool)item) ? 1 : 0;
				}
				else if (item.GetType().Name == "SqlGeography") //SqlGeography is a CLR Type
				{
					p.GetType().GetProperty("UdtTypeName").SetValue(p, "geography", null); //geography is the equivalent SQL Server Type
					p.Value = item;
				}

				else if (item.GetType().Name == "SqlGeometry") //SqlGeometry is a CLR Type
				{
					p.GetType().GetProperty("UdtTypeName").SetValue(p, "geometry", null); //geography is the equivalent SQL Server Type
					p.Value = item;
				}
				else
				{
					p.Value = item;
				}
			}

			cmd.Parameters.Add(p);
		}

		// Create a command
		static readonly Regex rxParamsPrefix = new Regex(@"(?<!@)@\w+", RegexOptions.Compiled);
		public IDbCommand CreateCommand(IDbConnection connection, string sql, params object[] args)
		{
			// Perform named argument replacements
			if (EnableNamedParams)
			{
				var new_args = new List<object>();
				sql = ProcessParams(sql, args, new_args);
				args = new_args.ToArray();
			}

			// Perform parameter prefix replacements
			if (_paramPrefix != "@")
				sql = rxParamsPrefix.Replace(sql, m => _paramPrefix + m.Value.Substring(1));
			sql = sql.Replace("@@", "@");		   // <- double @@ escapes a single @

			// Create the command and add parameters
			IDbCommand cmd = connection.CreateCommand();
			cmd.Connection = connection;
			cmd.CommandText = sql;
			cmd.Transaction = _transaction;
			foreach (var item in args)
			{
				AddParam(cmd, item, _paramPrefix);
			}

			if (_dbType == DBType.Oracle)
			{
				cmd.GetType().GetProperty("BindByName").SetValue(cmd, true, null);
			}

			if (!String.IsNullOrEmpty(sql))
				DoPreExecute(cmd);

			return cmd;
		}

		// Override this to log/capture exceptions
		public virtual void OnException(Exception x)
		{
			System.Diagnostics.Debug.WriteLine(x.ToString());
			System.Diagnostics.Debug.WriteLine(LastCommand);
		}

		// Override this to log commands, or modify command before execution
		public virtual IDbConnection OnConnectionOpened(IDbConnection conn) { return conn; }
		public virtual void OnConnectionClosing(IDbConnection conn) { }
		public virtual void OnExecutingCommand(IDbCommand cmd) { }
		public virtual void OnExecutedCommand(IDbCommand cmd) { }

		// Execute a non-query command
		public int Execute(string sql, params object[] args)
		{
			try
			{
				OpenSharedConnection();
				try
				{
					using (var cmd = CreateCommand(_sharedConnection, sql, args))
					{
						var retv=cmd.ExecuteNonQueryWithRetry();
						OnExecutedCommand(cmd);
						return retv;
					}
				}
				finally
				{
					CloseSharedConnection();
				}
			}
			catch (Exception x)
			{
				OnException(x);
				throw;
			}
		}

		public int Execute(Sql sql)
		{
			return Execute(sql.SQL, sql.Arguments);
		}

		// Execute and cast a scalar property
		public T ExecuteScalar<T>(string sql, params object[] args)
		{
			try
			{
				OpenSharedConnection();
				try
				{
					using (var cmd = CreateCommand(_sharedConnection, sql, args))
					{
						object val = cmd.ExecuteScalarWithRetry();
						OnExecutedCommand(cmd);

                        if (val == null || val == DBNull.Value)
                            return default(T);

                        Type t = typeof(T);
                        Type u = Nullable.GetUnderlyingType(t);

                        return (T)Convert.ChangeType(val, u ?? t);
					}
				}
				finally
				{
					CloseSharedConnection();
				}
			}
			catch (Exception x)
			{
				OnException(x);
				throw;
			}
		}

		public T ExecuteScalar<T>(Sql sql)
		{
			return ExecuteScalar<T>(sql.SQL, sql.Arguments);
		}

		static readonly Regex rxSelect = new Regex(@"\A\s*(SELECT|EXECUTE|CALL)\s", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
		static readonly Regex rxFrom = new Regex(@"\A\s*FROM\s", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
		string AddSelectClause<T>(string sql)
		{
			if (sql.StartsWith(";"))
				return sql.Substring(1);

			if (!rxSelect.IsMatch(sql))
			{
				var pd = PocoData.ForType(typeof(T));
				var tableName = EscapeTableName(pd.TableInfo.TableName);
				string cols = string.Join(", ", (from c in pd.QueryColumns select tableName + "." + EscapeSqlIdentifier(c)).ToArray());
				if (!rxFrom.IsMatch(sql))
					sql = string.Format("SELECT {0} FROM {1} {2}", cols, tableName, sql);
				else
					sql = string.Format("SELECT {0} {1}", cols, sql);
			}
			return sql;
		}

		public bool EnableAutoSelect { get; set; }
		public bool EnableNamedParams { get; set; }
		public bool ForceDateTimesToUtc { get; set; }

		// Return a typed list of pocos
		public List<T> Fetch<T>(string sql, params object[] args)
		{
			return Query<T>(sql, args).ToList();
		}

		public List<T> Fetch<T>(Sql sql)
		{
			return Fetch<T>(sql.SQL, sql.Arguments);
		}

		static readonly Regex rxColumns = new Regex(@"\A\s*SELECT\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\bFROM\b", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
		static readonly Regex rxOrderBy = new Regex(@"\bORDER\s+BY\s+(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
	    static readonly Regex rxDistinct = new Regex(@"\ADISTINCT\s", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
		public static bool SplitSqlForPaging(string sql, out string sqlCount, out string sqlSelectRemoved, out string sqlOrderBy)
		{
			sqlSelectRemoved = null;
			sqlCount = null;
			sqlOrderBy = null;

			// Extract the columns from "SELECT <whatever> FROM"
			var m = rxColumns.Match(sql);
			if (!m.Success)
				return false;

			// Save column list and replace with COUNT(*)
			Group g = m.Groups[1];
			sqlSelectRemoved = sql.Substring(g.Index);

			if (rxDistinct.IsMatch(sqlSelectRemoved))
				sqlCount = sql.Substring(0, g.Index) + "COUNT(" + m.Groups[1].ToString().Trim() + ") " + sql.Substring(g.Index + g.Length);
			else
				sqlCount = sql.Substring(0, g.Index) + "COUNT(*) " + sql.Substring(g.Index + g.Length);
            
		    // Look for an "ORDER BY <whatever>" clause
            m = rxOrderBy.Match(sqlCount);
			if (!m.Success)
			{
				sqlOrderBy = null;
			}
			else
			{
				g = m.Groups[0];
				sqlOrderBy = g.ToString();
				sqlCount = sqlCount.Substring(0, g.Index) + sqlCount.Substring(g.Index + g.Length);
			}
            return true;
		}

	    /// <summary>
	    /// NOTE: This is a custom mod of PetaPoco!! This builds the paging sql for different db providers
	    /// </summary>
	    /// <param name="sql"></param>
	    /// <param name="sqlSelectRemoved"></param>
	    /// <param name="sqlOrderBy"></param>
	    /// <param name="args"></param>
	    /// <param name="sqlPage"></param>
	    /// <param name="databaseType"></param>
	    /// <param name="skip"></param>
	    /// <param name="take"></param>
	    internal virtual void BuildSqlDbSpecificPagingQuery(DBType databaseType, long skip, long take, string sql, string sqlSelectRemoved, string sqlOrderBy, ref object[] args, out string sqlPage)
	    {
            // this is overriden in UmbracoDatabase, and if running SqlServer >=2012, the database type
            // is switched from SqlServer to SqlServerCE in order to use the better paging syntax that
            // SqlCE supports, and SqlServer >=2012 too.
            // so the first case is actually for SqlServer <2012, and second case is CE *and* SqlServer >=2012

            if (databaseType == DBType.SqlServer || databaseType == DBType.Oracle)
            {
                sqlSelectRemoved = rxOrderBy.Replace(sqlSelectRemoved, "");
                if (rxDistinct.IsMatch(sqlSelectRemoved))
                {
                    sqlSelectRemoved = "peta_inner.* FROM (SELECT " + sqlSelectRemoved + ") peta_inner";
                }

                // split to ensure that peta_rn is the last field to be selected, else Page<int> would fail
                // the resulting sql is not perfect, NPoco has a much nicer way to do it, but it would require
                // importing large parts of NPoco
                var pos = sqlSelectRemoved.IndexOf("FROM");
                var sqlColumns = sqlSelectRemoved.Substring(0, pos);
                var sqlFrom = sqlSelectRemoved.Substring(pos);

                sqlPage = string.Format("SELECT * FROM (SELECT {0}, ROW_NUMBER() OVER ({1}) peta_rn {2}) peta_paged WHERE peta_rn>@{3} AND peta_rn<=@{4}",
                                        sqlColumns, sqlOrderBy ?? "ORDER BY (SELECT NULL)", sqlFrom, args.Length, args.Length + 1);
                args = args.Concat(new object[] { skip, skip + take }).ToArray();
            }
            else if (databaseType == DBType.SqlServerCE)
            {
                sqlPage = string.Format("{0}\nOFFSET @{1} ROWS FETCH NEXT @{2} ROWS ONLY", sql, args.Length, args.Length + 1);
                args = args.Concat(new object[] { skip, take }).ToArray();
            }
            else
            {
                sqlPage = string.Format("{0}\nLIMIT @{1} OFFSET @{2}", sql, args.Length, args.Length + 1);
                args = args.Concat(new object[] { take, skip }).ToArray();
            }
        }

		public void BuildPageQueries<T>(long skip, long take, string sql, ref object[] args, out string sqlCount, out string sqlPage)
		{
			// Add auto select clause
			if (EnableAutoSelect)
				sql = AddSelectClause<T>(sql);

			// Split the SQL into the bits we need
			string sqlSelectRemoved, sqlOrderBy;
			if (SplitSqlForPaging(sql, out sqlCount, out sqlSelectRemoved, out sqlOrderBy) == false)
				throw new Exception("Unable to parse SQL statement for paged query");
			if (_dbType == DBType.Oracle && sqlSelectRemoved.StartsWith("*"))
                throw new Exception("Query must alias '*' when performing a paged query.\neg. select t.* from table t order by t.id");

		    BuildSqlDbSpecificPagingQuery(_dbType, skip, take, sql, sqlSelectRemoved, sqlOrderBy, ref args, out sqlPage);
		}

		// Fetch a page
		public Page<T> Page<T>(long page, long itemsPerPage, string sql, params object[] args)
		{
			string sqlCount, sqlPage;
			BuildPageQueries<T>((page-1)*itemsPerPage, itemsPerPage, sql, ref args, out sqlCount, out sqlPage);

			// Save the one-time command time out and use it for both queries
			int saveTimeout = OneTimeCommandTimeout;

			// Setup the paged result
			var result = new Page<T>();
			result.CurrentPage = page;
			result.ItemsPerPage = itemsPerPage;
			result.TotalItems = ExecuteScalar<long>(sqlCount, args);
		    result.TotalPages = result.TotalItems / itemsPerPage;
			if ((result.TotalItems % itemsPerPage) != 0)
				result.TotalPages++;

			OneTimeCommandTimeout = saveTimeout;

			// Get the records
			result.Items = Fetch<T>(sqlPage, args);

			// Done
			return result;
		}

		public Page<T> Page<T>(long page, long itemsPerPage, Sql sql)
		{
			return Page<T>(page, itemsPerPage, sql.SQL, sql.Arguments);
		}


		public List<T> Fetch<T>(long page, long itemsPerPage, string sql, params object[] args)
		{
			return SkipTake<T>((page - 1) * itemsPerPage, itemsPerPage, sql, args);
		}

		public List<T> Fetch<T>(long page, long itemsPerPage, Sql sql)
		{
			return SkipTake<T>((page - 1) * itemsPerPage, itemsPerPage, sql.SQL, sql.Arguments);
		}

		public List<T> SkipTake<T>(long skip, long take, string sql, params object[] args)
		{
			string sqlCount, sqlPage;
			BuildPageQueries<T>(skip, take, sql, ref args, out sqlCount, out sqlPage);
			return Fetch<T>(sqlPage, args);
		}

		public List<T> SkipTake<T>(long skip, long take, Sql sql)
		{
			return SkipTake<T>(skip, take, sql.SQL, sql.Arguments);
		}

		// Return an enumerable collection of pocos
		public IEnumerable<T> Query<T>(string sql, params object[] args)
		{
			if (EnableAutoSelect)
				sql = AddSelectClause<T>(sql);

			OpenSharedConnection();
			try
			{
				using (var cmd = CreateCommand(_sharedConnection, sql, args))
				{
					IDataReader r;
					var pd = PocoData.ForType(typeof(T));
					try
					{
						r = cmd.ExecuteReaderWithRetry();
						OnExecutedCommand(cmd);
					}
					catch (Exception x)
					{
						OnException(x);
						throw;
					}
					var factory = pd.GetFactory(cmd.CommandText, _sharedConnection.ConnectionString, ForceDateTimesToUtc, 0, r.FieldCount, r) as Func<IDataReader, T>;
					using (r)
					{
						while (true)
						{
							T poco;
							try
							{
								if (!r.Read())
									yield break;
								poco = factory(r);
							}
							catch (Exception x)
							{
								OnException(x);
								throw;
							}

							yield return poco;
						}
					}
				}
			}
			finally
			{
				CloseSharedConnection();
			}
		}

		// Multi Fetch
		public List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args) { return Query<T1, T2, TRet>(cb, sql, args).ToList(); }
		public List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args) { return Query<T1, T2, T3, TRet>(cb, sql, args).ToList(); }
		public List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args) { return Query<T1, T2, T3, T4, TRet>(cb, sql, args).ToList(); }
        public List<TRet> Fetch<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> cb, string sql, params object[] args) { return Query<T1, T2, T3, T4, T5, TRet>(cb, sql, args).ToList(); }

		// Multi Query
		public IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2) }, cb, sql, args); }
		public IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3)}, cb, sql, args); }
		public IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4)}, cb, sql, args); }
        public IEnumerable<TRet> Query<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> cb, string sql, params object[] args) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, cb, sql, args); }

		// Multi Fetch (SQL builder)
		public List<TRet> Fetch<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql) { return Query<T1, T2, TRet>(cb, sql.SQL, sql.Arguments).ToList(); }
		public List<TRet> Fetch<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql) { return Query<T1, T2, T3, TRet>(cb, sql.SQL, sql.Arguments).ToList(); }
		public List<TRet> Fetch<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql) { return Query<T1, T2, T3, T4, TRet>(cb, sql.SQL, sql.Arguments).ToList(); }
        public List<TRet> Fetch<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> cb, Sql sql) { return Query<T1, T2, T3, T4, T5, TRet>(cb, sql.SQL, sql.Arguments).ToList(); }

		// Multi Query (SQL builder)
		public IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, Sql sql) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2) }, cb, sql.SQL, sql.Arguments); }
		public IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, Sql sql) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, cb, sql.SQL, sql.Arguments); }
		public IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, Sql sql) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, cb, sql.SQL, sql.Arguments); }
        public IEnumerable<TRet> Query<T1, T2, T3, T4, T5, TRet>(Func<T1, T2, T3, T4, T5, TRet> cb, Sql sql) { return Query<TRet>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, cb, sql.SQL, sql.Arguments); }

		// Multi Fetch (Simple)
		public List<T1> Fetch<T1, T2>(string sql, params object[] args) { return Query<T1, T2>(sql, args).ToList(); }
		public List<T1> Fetch<T1, T2, T3>(string sql, params object[] args) { return Query<T1, T2, T3>(sql, args).ToList(); }
		public List<T1> Fetch<T1, T2, T3, T4>(string sql, params object[] args) { return Query<T1, T2, T3, T4>(sql, args).ToList(); }
        public List<T1> Fetch<T1, T2, T3, T4, T5>(string sql, params object[] args) { return Query<T1, T2, T3, T4, T5>(sql, args).ToList(); }

		// Multi Query (Simple)
		public IEnumerable<T1> Query<T1, T2>(string sql, params object[] args) { return Query<T1>(new Type[] { typeof(T1), typeof(T2) }, null, sql, args); }
		public IEnumerable<T1> Query<T1, T2, T3>(string sql, params object[] args) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, null, sql, args); }
		public IEnumerable<T1> Query<T1, T2, T3, T4>(string sql, params object[] args) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, null, sql, args); }
        public IEnumerable<T1> Query<T1, T2, T3, T4, T5>(string sql, params object[] args) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, null, sql, args); }

		// Multi Fetch (Simple) (SQL builder)
		public List<T1> Fetch<T1, T2>(Sql sql) { return Query<T1, T2>(sql.SQL, sql.Arguments).ToList(); }
		public List<T1> Fetch<T1, T2, T3>(Sql sql) { return Query<T1, T2, T3>(sql.SQL, sql.Arguments).ToList(); }
		public List<T1> Fetch<T1, T2, T3, T4>(Sql sql) { return Query<T1, T2, T3, T4>(sql.SQL, sql.Arguments).ToList(); }
        public List<T1> Fetch<T1, T2, T3, T4, T5>(Sql sql) { return Query<T1, T2, T3, T4, T5>(sql.SQL, sql.Arguments).ToList(); }

		// Multi Query (Simple) (SQL builder)
		public IEnumerable<T1> Query<T1, T2>(Sql sql) { return Query<T1>(new Type[] { typeof(T1), typeof(T2) }, null, sql.SQL, sql.Arguments); }
		public IEnumerable<T1> Query<T1, T2, T3>(Sql sql) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3) }, null, sql.SQL, sql.Arguments); }
		public IEnumerable<T1> Query<T1, T2, T3, T4>(Sql sql) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) }, null, sql.SQL, sql.Arguments); }
        public IEnumerable<T1> Query<T1, T2, T3, T4, T5>(Sql sql) { return Query<T1>(new Type[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) }, null, sql.SQL, sql.Arguments); }

		// Automagically guess the property relationships between various POCOs and create a delegate that will set them up
		Delegate GetAutoMapper(Type[] types)
		{
			// Build a key
			var kb = new StringBuilder();
			foreach (var t in types)
			{
				kb.Append(t.ToString());
				kb.Append(":");
			}
			var key = kb.ToString();

			// Check cache
			RWLock.EnterReadLock();
			try
			{
				Delegate mapper;
				if (AutoMappers.TryGetValue(key, out mapper))
					return mapper;
			}
			finally
			{
				RWLock.ExitReadLock();
			}

			// Create it
			RWLock.EnterWriteLock();
			try
			{
				// Try again
				Delegate mapper;
				if (AutoMappers.TryGetValue(key, out mapper))
					return mapper;

				// Create a method
				var m = new DynamicMethod("petapoco_automapper", types[0], types, true);
				var il = m.GetILGenerator();

				for (int i = 1; i < types.Length; i++)
				{
					bool handled = false;
					for (int j = i - 1; j >= 0; j--)
					{
						// Find the property
						var candidates = from p in types[j].GetProperties() where p.PropertyType == types[i] select p;
						if (candidates.Count() == 0)
							continue;
						if (candidates.Count() > 1)
							throw new InvalidOperationException(string.Format("Can't auto join {0} as {1} has more than one property of type {0}", types[i], types[j]));

						// Generate code
						il.Emit(OpCodes.Ldarg_S, j);
						il.Emit(OpCodes.Ldarg_S, i);
						il.Emit(OpCodes.Callvirt, candidates.First().GetSetMethod(true));
						handled = true;
					}

					if (!handled)
						throw new InvalidOperationException(string.Format("Can't auto join {0}", types[i]));
				}

				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ret);

				// Cache it
				var del = m.CreateDelegate(Expression.GetFuncType(types.Concat(types.Take(1)).ToArray()));
				AutoMappers.Add(key, del);
				return del;
			}
			finally
			{
				RWLock.ExitWriteLock();
			}
		}

		// Find the split point in a result set for two different pocos and return the poco factory for the first
		Delegate FindSplitPoint(Type typeThis, Type typeNext, string sql, IDataReader r, ref int pos)
		{
			// Last?
			if (typeNext == null)
				return PocoData.ForType(typeThis).GetFactory(sql, _sharedConnection.ConnectionString, ForceDateTimesToUtc, pos, r.FieldCount - pos, r);

			// Get PocoData for the two types
			PocoData pdThis = PocoData.ForType(typeThis);
			PocoData pdNext = PocoData.ForType(typeNext);

			// Find split point
			int firstColumn = pos;
			var usedColumns = new Dictionary<string, bool>();
			for (; pos < r.FieldCount; pos++)
			{
				// Split if field name has already been used, or if the field doesn't exist in current poco but does in the next
				string fieldName = r.GetName(pos);
				if (usedColumns.ContainsKey(fieldName) || (!pdThis.Columns.ContainsKey(fieldName) && pdNext.Columns.ContainsKey(fieldName)))
				{
					return pdThis.GetFactory(sql, _sharedConnection.ConnectionString, ForceDateTimesToUtc, firstColumn, pos - firstColumn, r);
				}
				usedColumns.Add(fieldName, true);
			}

			throw new InvalidOperationException(string.Format("Couldn't find split point between {0} and {1}", typeThis, typeNext));
		}


		// Instance data used by the Multipoco factory delegate - essentially a list of the nested poco factories to call
		public class MultiPocoFactory
		{

			public MultiPocoFactory(IEnumerable<Delegate> dels)
			{
				Delegates = new List<Delegate>(dels);
			}
			private List<Delegate> Delegates { get; set; }
			private Delegate GetItem(int index) { return Delegates[index]; }

			/// <summary>
			/// Calls the delegate at the specified index and returns its values
			/// </summary>
			/// <param name="index"></param>
			/// <param name="reader"></param>
			/// <returns></returns>
			private object CallDelegate(int index, IDataReader reader)
			{
				var d = GetItem(index);
				var output = d.DynamicInvoke(reader);
				return output;
			}

			/// <summary>
			/// Calls the callback delegate and passes in the output of all delegates as the parameters
			/// </summary>
			/// <typeparam name="TRet"></typeparam>
			/// <param name="callback"></param>
			/// <param name="dr"></param>
			/// <param name="count"></param>
			/// <returns></returns>
			public TRet CallCallback<TRet>(Delegate callback, IDataReader dr, int count)
			{
				var args = new List<object>();
				for(var i = 0;i<count;i++)
				{
					args.Add(CallDelegate(i, dr));
				}
				return (TRet)callback.DynamicInvoke(args.ToArray());
			}
		}

		// Create a multi-poco factory
		Func<IDataReader, Delegate, TRet> CreateMultiPocoFactory<TRet>(Type[] types, string sql, IDataReader r)
		{
			// Call each delegate
			var dels = new List<Delegate>();
			int pos = 0;
			for (int i=0; i<types.Length; i++)
			{
				// Add to list of delegates to call
				var del = FindSplitPoint(types[i], i + 1 < types.Length ? types[i + 1] : null, sql, r, ref pos);
				dels.Add(del);
			}

			var mpFactory = new MultiPocoFactory(dels);
			return (reader, arg3) => mpFactory.CallCallback<TRet>(arg3, reader, types.Length);
		}

		// Various cached stuff
		static Dictionary<string, object> MultiPocoFactories = new Dictionary<string, object>();
		static Dictionary<string, Delegate> AutoMappers = new Dictionary<string, Delegate>();
		static System.Threading.ReaderWriterLockSlim RWLock = new System.Threading.ReaderWriterLockSlim();

		// Get (or create) the multi-poco factory for a query
		Func<IDataReader, Delegate, TRet> GetMultiPocoFactory<TRet>(Type[] types, string sql, IDataReader r)
		{
			// Build a key string  (this is crap, should address this at some point)
			var kb = new StringBuilder();
			kb.Append(typeof(TRet).ToString());
			kb.Append(":");
			foreach (var t in types)
			{
				kb.Append(":");
				kb.Append(t.ToString());
			}
			kb.Append(":"); kb.Append(_sharedConnection.ConnectionString);
			kb.Append(":"); kb.Append(ForceDateTimesToUtc);
			kb.Append(":"); kb.Append(sql);
			string key = kb.ToString();

			// Check cache
			RWLock.EnterReadLock();
			try
			{
				object oFactory;
				if (MultiPocoFactories.TryGetValue(key, out oFactory))
				{
					//mpFactory = oFactory;
					return (Func<IDataReader, Delegate, TRet>)oFactory;
				}
			}
			finally
			{
				RWLock.ExitReadLock();
			}

			// Cache it
			RWLock.EnterWriteLock();
			try
			{
				// Check again
				object oFactory; ;
				if (MultiPocoFactories.TryGetValue(key, out oFactory))
				{
					return (Func<IDataReader, Delegate, TRet>)oFactory;
				}

				// Create the factory
				var factory = CreateMultiPocoFactory<TRet>(types, sql, r);

				MultiPocoFactories.Add(key, factory);
				return factory;
			}
			finally
			{
				RWLock.ExitWriteLock();
			}

		}

		// Actual implementation of the multi-poco query
		public IEnumerable<TRet> Query<TRet>(Type[] types, Delegate cb, string sql, params object[] args)
		{
			OpenSharedConnection();
			try
			{
				using (var cmd = CreateCommand(_sharedConnection, sql, args))
				{
					IDataReader r;
					try
					{
						r = cmd.ExecuteReaderWithRetry();
						OnExecutedCommand(cmd);
					}
					catch (Exception x)
					{
						OnException(x);
						throw;
					}
					var factory = GetMultiPocoFactory<TRet>(types, sql, r);
					if (cb == null)
						cb = GetAutoMapper(types.ToArray());
					bool bNeedTerminator=false;
					using (r)
					{
						while (true)
						{
							TRet poco;
							try
							{
								if (!r.Read())
									break;
								poco = factory(r, cb);
							}
							catch (Exception x)
							{
								OnException(x);
								throw;
							}

							if (poco != null)
								yield return poco;
							else
								bNeedTerminator = true;
						}
						if (bNeedTerminator)
						{
							var poco = (TRet)(cb as Delegate).DynamicInvoke(new object[types.Length]);
							if (poco != null)
								yield return poco;
							else
								yield break;
						}
					}
				}
			}
			finally
			{
				CloseSharedConnection();
			}
		}


		public IEnumerable<T> Query<T>(Sql sql)
		{
			return Query<T>(sql.SQL, sql.Arguments);
		}

		public bool Exists<T>(object primaryKey)
		{
			return FirstOrDefault<T>(string.Format("WHERE {0}=@0", EscapeSqlIdentifier(PocoData.ForType(typeof(T)).TableInfo.PrimaryKey)), primaryKey) != null;
		}
		public T Single<T>(object primaryKey)
		{
			return Single<T>(string.Format("WHERE {0}=@0", EscapeSqlIdentifier(PocoData.ForType(typeof(T)).TableInfo.PrimaryKey)), primaryKey);
		}
		public T SingleOrDefault<T>(object primaryKey)
		{
			return SingleOrDefault<T>(string.Format("WHERE {0}=@0", EscapeSqlIdentifier(PocoData.ForType(typeof(T)).TableInfo.PrimaryKey)), primaryKey);
		}
		public T Single<T>(string sql, params object[] args)
		{
			return Query<T>(sql, args).Single();
		}
		public T SingleOrDefault<T>(string sql, params object[] args)
		{
			return Query<T>(sql, args).SingleOrDefault();
		}
		public T First<T>(string sql, params object[] args)
		{
			return Query<T>(sql, args).First();
		}
		public T FirstOrDefault<T>(string sql, params object[] args)
		{
			return Query<T>(sql, args).FirstOrDefault();
		}

		public T Single<T>(Sql sql)
		{
			return Query<T>(sql).Single();
		}
		public T SingleOrDefault<T>(Sql sql)
		{
			return Query<T>(sql).SingleOrDefault();
		}
		public T First<T>(Sql sql)
		{
			return Query<T>(sql).First();
		}
		public T FirstOrDefault<T>(Sql sql)
		{
			return Query<T>(sql).FirstOrDefault();
		}

		public string EscapeTableName(string str)
		{
			// Assume table names with "dot", or opening sq is already escaped
			return str.IndexOf('.') >= 0 ? str : EscapeSqlIdentifier(str);
		}
		public string EscapeSqlIdentifier(string str)
		{
			switch (_dbType)
			{
				case DBType.MySql:
					return string.Format("`{0}`", str);

				case DBType.PostgreSQL:
				case DBType.Oracle:
					return string.Format("\"{0}\"", str);

				default:
					return string.Format("[{0}]", str);
			}
		}

		public object Insert(string tableName, string primaryKeyName, object poco)
		{
			return Insert(tableName, primaryKeyName, true, poco);
		}

		// Insert a poco into a table.  If the poco has a property with the same name
		// as the primary key the id of the new record is assigned to it.  Either way,
		// the new id is returned.
		public object Insert(string tableName, string primaryKeyName, bool autoIncrement, object poco)
		{
			try
			{
				OpenSharedConnection();
				try
				{
					using (var cmd = CreateCommand(_sharedConnection, ""))
					{
						var pd = PocoData.ForObject(poco, primaryKeyName);
						var names = new List<string>();
						var values = new List<string>();
						var index = 0;
						foreach (var i in pd.Columns)
						{
							// Don't insert result columns
							if (i.Value.ResultColumn)
								continue;

							// Don't insert the primary key (except under oracle where we need bring in the next sequence value)
							if (autoIncrement && primaryKeyName != null && string.Compare(i.Key, primaryKeyName, true)==0)
							{
								if (_dbType == DBType.Oracle && !string.IsNullOrEmpty(pd.TableInfo.SequenceName))
								{
									names.Add(i.Key);
									values.Add(string.Format("{0}.nextval", pd.TableInfo.SequenceName));
								}
								continue;
							}

							names.Add(EscapeSqlIdentifier(i.Key));
							values.Add(string.Format("{0}{1}", _paramPrefix, index++));
							AddParam(cmd, i.Value.GetValue(poco), _paramPrefix);
						}

						cmd.CommandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
								EscapeTableName(tableName),
								string.Join(",", names.ToArray()),
								string.Join(",", values.ToArray())
								);

						if (!autoIncrement)
						{
							DoPreExecute(cmd);
							cmd.ExecuteNonQueryWithRetry();
							OnExecutedCommand(cmd);
							return true;
						}


						object id;
						switch (_dbType)
						{
							case DBType.SqlServerCE:
								DoPreExecute(cmd);
								cmd.ExecuteNonQueryWithRetry();
								OnExecutedCommand(cmd);
								id = ExecuteScalar<object>("SELECT @@@IDENTITY AS NewID;");
								break;
							case DBType.SqlServer:
								cmd.CommandText += ";\nSELECT SCOPE_IDENTITY() AS NewID;";
								DoPreExecute(cmd);
								id = cmd.ExecuteScalarWithRetry();
								OnExecutedCommand(cmd);
								break;
							case DBType.PostgreSQL:
								if (primaryKeyName != null)
								{
									cmd.CommandText += string.Format("returning {0} as NewID", EscapeSqlIdentifier(primaryKeyName));
									DoPreExecute(cmd);
									id = cmd.ExecuteScalarWithRetry();
								}
								else
								{
									id = -1;
									DoPreExecute(cmd);
									cmd.ExecuteNonQueryWithRetry();
								}
								OnExecutedCommand(cmd);
								break;
							case DBType.Oracle:
								if (primaryKeyName != null)
								{
									cmd.CommandText += string.Format(" returning {0} into :newid", EscapeSqlIdentifier(primaryKeyName));
									var param = cmd.CreateParameter();
									param.ParameterName = ":newid";
									param.Value = DBNull.Value;
									param.Direction = ParameterDirection.ReturnValue;
									param.DbType = DbType.Int64;
									cmd.Parameters.Add(param);
									DoPreExecute(cmd);
									cmd.ExecuteNonQueryWithRetry();
									id = param.Value;
								}
								else
								{
									id = -1;
									DoPreExecute(cmd);
									cmd.ExecuteNonQueryWithRetry();
								}
								OnExecutedCommand(cmd);
								break;
                            case DBType.SQLite:
                                if (primaryKeyName != null)
                                {
                                    cmd.CommandText += ";\nSELECT last_insert_rowid();";
                                    DoPreExecute(cmd);
                                    id = cmd.ExecuteScalarWithRetry();
                                }
                                else
                                {
                                    id = -1;
                                    DoPreExecute(cmd);
                                    cmd.ExecuteNonQueryWithRetry();
                                }
                                OnExecutedCommand(cmd);
                                break;
							default:
								cmd.CommandText += ";\nSELECT @@IDENTITY AS NewID;";
								DoPreExecute(cmd);
								id = cmd.ExecuteScalarWithRetry();
								OnExecutedCommand(cmd);
								break;
						}


						// Assign the ID back to the primary key property
						if (primaryKeyName != null)
						{
							PocoColumn pc;
							if (pd.Columns.TryGetValue(primaryKeyName, out pc))
							{
								pc.SetValue(poco, pc.ChangeType(id));
							}
						}

						return id;
					}
				}
				finally
				{
					CloseSharedConnection();
				}
			}
			catch (Exception x)
			{
				OnException(x);
				throw;
			}
		}

		// Insert an annotated poco object
		public object Insert(object poco)
		{
			var pd = PocoData.ForType(poco.GetType());
			return Insert(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, pd.TableInfo.AutoIncrement, poco);
		}

		public int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue)
		{
			return Update(tableName, primaryKeyName, poco, primaryKeyValue, null);
		}


		// Update a record with values from a poco.  primary key value can be either supplied or read from the poco
		public int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue, IEnumerable<string> columns)
		{
			try
			{
				OpenSharedConnection();
				try
				{
					using (var cmd = CreateCommand(_sharedConnection, ""))
					{
						var sb = new StringBuilder();
						var index = 0;
						var pd = PocoData.ForObject(poco,primaryKeyName);
						if (columns == null)
						{
							foreach (var i in pd.Columns)
							{
								// Don't update the primary key, but grab the value if we don't have it
								if (string.Compare(i.Key, primaryKeyName, true) == 0)
								{
									if (primaryKeyValue == null)
										primaryKeyValue = i.Value.GetValue(poco);
									continue;
								}

								// Dont update result only columns
								if (i.Value.ResultColumn)
									continue;

								// Build the sql
								if (index > 0)
									sb.Append(", ");
								sb.AppendFormat("{0} = {1}{2}", EscapeSqlIdentifier(i.Key), _paramPrefix, index++);

								// Store the parameter in the command
								AddParam(cmd, i.Value.GetValue(poco), _paramPrefix);
							}
						}
						else
						{
							foreach (var colname in columns)
							{
								var pc = pd.Columns[colname];

								// Build the sql
								if (index > 0)
									sb.Append(", ");
								sb.AppendFormat("{0} = {1}{2}", EscapeSqlIdentifier(colname), _paramPrefix, index++);

								// Store the parameter in the command
								AddParam(cmd, pc.GetValue(poco), _paramPrefix);
							}

							// Grab primary key value
							if (primaryKeyValue == null)
							{
								var pc = pd.Columns[primaryKeyName];
								primaryKeyValue = pc.GetValue(poco);
							}

						}

						cmd.CommandText = string.Format("UPDATE {0} SET {1} WHERE {2} = {3}{4}",
											EscapeTableName(tableName), sb.ToString(), EscapeSqlIdentifier(primaryKeyName), _paramPrefix, index++);
						AddParam(cmd, primaryKeyValue, _paramPrefix);

						DoPreExecute(cmd);

						// Do it
						var retv=cmd.ExecuteNonQueryWithRetry();
						OnExecutedCommand(cmd);
						return retv;
					}
				}
				finally
				{
					CloseSharedConnection();
				}
			}
			catch (Exception x)
			{
				OnException(x);
				throw;
			}
		}

		public int Update(string tableName, string primaryKeyName, object poco)
		{
			return Update(tableName, primaryKeyName, poco, null);
		}

		public int Update(string tableName, string primaryKeyName, object poco, IEnumerable<string> columns)
		{
			return Update(tableName, primaryKeyName, poco, null, columns);
		}

		public int Update(object poco, IEnumerable<string> columns)
		{
			return Update(poco, null, columns);
		}

		public int Update(object poco)
		{
			return Update(poco, null, null);
		}

		public int Update(object poco, object primaryKeyValue)
		{
			return Update(poco, primaryKeyValue, null);
		}
		public int Update(object poco, object primaryKeyValue, IEnumerable<string> columns)
		{
			var pd = PocoData.ForType(poco.GetType());
			return Update(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, poco, primaryKeyValue, columns);
		}

		public int Update<T>(string sql, params object[] args)
		{
			var pd = PocoData.ForType(typeof(T));
			return Execute(string.Format("UPDATE {0} {1}", EscapeTableName(pd.TableInfo.TableName), sql), args);
		}

		public int Update<T>(Sql sql)
		{
			var pd = PocoData.ForType(typeof(T));
			return Execute(new Sql(string.Format("UPDATE {0}", EscapeTableName(pd.TableInfo.TableName))).Append(sql));
		}

		public int Delete(string tableName, string primaryKeyName, object poco)
		{
			return Delete(tableName, primaryKeyName, poco, null);
		}

		public int Delete(string tableName, string primaryKeyName, object poco, object primaryKeyValue)
		{
			// If primary key value not specified, pick it up from the object
			if (primaryKeyValue == null)
			{
				var pd = PocoData.ForObject(poco,primaryKeyName);
				PocoColumn pc;
				if (pd.Columns.TryGetValue(primaryKeyName, out pc))
				{
					primaryKeyValue = pc.GetValue(poco);
				}
			}

			// Do it
			var sql = string.Format("DELETE FROM {0} WHERE {1}=@0", EscapeTableName(tableName), EscapeSqlIdentifier(primaryKeyName));
			return Execute(sql, primaryKeyValue);
		}

		public int Delete(object poco)
		{
			var pd = PocoData.ForType(poco.GetType());
			return Delete(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, poco);
		}

		public int Delete<T>(object pocoOrPrimaryKey)
		{
			if (pocoOrPrimaryKey.GetType() == typeof(T))
				return Delete(pocoOrPrimaryKey);
			var pd = PocoData.ForType(typeof(T));
			return Delete(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, null, pocoOrPrimaryKey);
		}

		public int Delete<T>(string sql, params object[] args)
		{
			var pd = PocoData.ForType(typeof(T));
			return Execute(string.Format("DELETE FROM {0} {1}", EscapeTableName(pd.TableInfo.TableName), sql), args);
		}

		public int Delete<T>(Sql sql)
		{
			var pd = PocoData.ForType(typeof(T));
			return Execute(new Sql(string.Format("DELETE FROM {0}", EscapeTableName(pd.TableInfo.TableName))).Append(sql));
		}

		// Check if a poco represents a new record
		public bool IsNew(string primaryKeyName, object poco)
		{
			var pd = PocoData.ForObject(poco, primaryKeyName);
			object pk;
			PocoColumn pc;
			if (pd.Columns.TryGetValue(primaryKeyName, out pc))
			{
				pk = pc.GetValue(poco);
			}
#if !PETAPOCO_NO_DYNAMIC
			else if (poco.GetType() == typeof(System.Dynamic.ExpandoObject))
			{
				return true;
			}
#endif
			else
			{
				var pi = poco.GetType().GetProperty(primaryKeyName);
				if (pi == null)
					throw new ArgumentException(string.Format("The object doesn't have a property matching the primary key column name '{0}'", primaryKeyName));
				pk = pi.GetValue(poco, null);
			}

			if (pk == null)
				return true;

			var type = pk.GetType();

			if (type.IsValueType)
			{
				// Common primary key types
				if (type == typeof(long))
					return (long)pk == 0;
				else if (type == typeof(ulong))
					return (ulong)pk == 0;
				else if (type == typeof(int))
					return (int)pk == 0;
				else if (type == typeof(uint))
					return (uint)pk == 0;

				// Create a default instance and compare
				return pk == Activator.CreateInstance(pk.GetType());
			}
			else
			{
				return pk == null;
			}
		}

		public bool IsNew(object poco)
		{
			var pd = PocoData.ForType(poco.GetType());
			if (!pd.TableInfo.AutoIncrement)
				throw new InvalidOperationException("IsNew() and Save() are only supported on tables with auto-increment/identity primary key columns");
			return IsNew(pd.TableInfo.PrimaryKey, poco);
		}

		// Insert new record or Update existing record
		public void Save(string tableName, string primaryKeyName, object poco)
		{
			if (IsNew(primaryKeyName, poco))
			{
				Insert(tableName, primaryKeyName, true, poco);
			}
			else
			{
				Update(tableName, primaryKeyName, poco);
			}
		}

		public void Save(object poco)
		{
			var pd = PocoData.ForType(poco.GetType());
			Save(pd.TableInfo.TableName, pd.TableInfo.PrimaryKey, poco);
		}

		public int CommandTimeout { get; set; }
		public int OneTimeCommandTimeout { get; set; }

		void DoPreExecute(IDbCommand cmd)
		{
			// Setup command timeout
			if (OneTimeCommandTimeout != 0)
			{
				cmd.CommandTimeout = OneTimeCommandTimeout;
				OneTimeCommandTimeout = 0;
			}
			else if (CommandTimeout!=0)
			{
				cmd.CommandTimeout = CommandTimeout;
			}

			// Call hook
			OnExecutingCommand(cmd);

			// Save it
			_lastSql = cmd.CommandText;
			_lastArgs = (from IDataParameter parameter in cmd.Parameters select parameter.Value).ToArray();
		}

		public string LastSQL { get { return _lastSql; } }
		public object[] LastArgs { get { return _lastArgs; } }
		public string LastCommand
		{
			get { return FormatCommand(_lastSql, _lastArgs); }
		}

		public string FormatCommand(IDbCommand cmd)
		{
			return FormatCommand(cmd.CommandText, (from IDataParameter parameter in cmd.Parameters select parameter.Value).ToArray());
		}

		public string FormatCommand(string sql, object[] args)
		{
			var sb = new StringBuilder();
			if (sql == null)
				return "";
			sb.Append(sql);
			if (args != null && args.Length > 0)
			{
				sb.Append("\n");
				for (int i = 0; i < args.Length; i++)
				{
					sb.AppendFormat("\t -> {0}{1} [{2}] = \"{3}\"\n", _paramPrefix, i, args[i].GetType().Name, args[i]);
				}
				sb.Remove(sb.Length - 1, 1);
			}
			return sb.ToString();
		}


		public static IMapper Mapper
		{
			get;
			set;
		}

		public class PocoColumn
		{
			public string ColumnName;
			public PropertyInfo PropertyInfo;
			public bool ResultColumn;
			public virtual void SetValue(object target, object val) { PropertyInfo.SetValue(target, val, null); }
			public virtual object GetValue(object target) { return PropertyInfo.GetValue(target, null); }
			public virtual object ChangeType(object val) { return Convert.ChangeType(val, PropertyInfo.PropertyType); }
		}
		public class ExpandoColumn : PocoColumn
		{
			public override void SetValue(object target, object val) { (target as IDictionary<string, object>)[ColumnName]=val; }
			public override object GetValue(object target)
			{
				object val=null;
				(target as IDictionary<string, object>).TryGetValue(ColumnName, out val);
				return val;
			}
			public override object ChangeType(object val) { return val; }
		}

        /// <summary>
        /// Container for a Memory cache object
        /// </summary>
        /// <remarks>
        /// Better to have one memory cache instance than many so it's memory management can be handled more effectively
        /// http://stackoverflow.com/questions/8463962/using-multiple-instances-of-memorycache
        /// </remarks>
        internal class ManagedCache
        {
            public ObjectCache GetCache()
            {
                return ObjectCache;
            }

            static readonly ObjectCache ObjectCache = new MemoryCache("NPoco");

        }

        public class PocoData
        {
            //USE ONLY FOR TESTING
            internal static bool UseLongKeys = false;
            //USE ONLY FOR TESTING - default is one hr
            internal static int SlidingExpirationSeconds = 3600;

			public static PocoData ForObject(object o, string primaryKeyName)
			{
				var t = o.GetType();
#if !PETAPOCO_NO_DYNAMIC
				if (t == typeof(System.Dynamic.ExpandoObject))
				{
					var pd = new PocoData();
					pd.TableInfo = new TableInfo();
					pd.Columns = new Dictionary<string, PocoColumn>(StringComparer.OrdinalIgnoreCase);
					pd.Columns.Add(primaryKeyName, new ExpandoColumn() { ColumnName = primaryKeyName });
					pd.TableInfo.PrimaryKey = primaryKeyName;
					pd.TableInfo.AutoIncrement = true;
					foreach (var col in (o as IDictionary<string, object>).Keys)
					{
						if (col!=primaryKeyName)
							pd.Columns.Add(col, new ExpandoColumn() { ColumnName = col });
					}
					return pd;
				}
				else
#endif
					return ForType(t);
			}

			public static PocoData ForType(Type t)
			{
#if !PETAPOCO_NO_DYNAMIC
				if (t == typeof(System.Dynamic.ExpandoObject))
					throw new InvalidOperationException("Can't use dynamic types with this method");
#endif
				// Check cache
				InnerLock.EnterReadLock();
				PocoData pd;
				try
				{
					if (m_PocoDatas.TryGetValue(t, out pd))
						return pd;
				}
				finally
				{
					InnerLock.ExitReadLock();
				}


				// Cache it
				InnerLock.EnterWriteLock();
				try
				{
					// Check again
					if (m_PocoDatas.TryGetValue(t, out pd))
						return pd;

					// Create it
					pd = new PocoData(t);

					m_PocoDatas.Add(t, pd);
				}
				finally
				{
					InnerLock.ExitWriteLock();
				}

				return pd;
			}

			public PocoData()
			{
			}

			public PocoData(Type t)
			{
				type = t;
				TableInfo=new TableInfo();

				// Get the table name
				var a = t.GetCustomAttributes(typeof(TableNameAttribute), true);
				TableInfo.TableName = a.Length == 0 ? t.Name : (a[0] as TableNameAttribute).Value;

				// Get the primary key
				a = t.GetCustomAttributes(typeof(PrimaryKeyAttribute), true);
				TableInfo.PrimaryKey = a.Length == 0 ? "ID" : (a[0] as PrimaryKeyAttribute).Value;
				TableInfo.SequenceName = a.Length == 0 ? null : (a[0] as PrimaryKeyAttribute).sequenceName;
				TableInfo.AutoIncrement = a.Length == 0 ? false : (a[0] as PrimaryKeyAttribute).autoIncrement;

				// Call column mapper
				if (Database.Mapper != null)
					Database.Mapper.GetTableInfo(t, TableInfo);

				// Work out bound properties
				bool ExplicitColumns = t.GetCustomAttributes(typeof(ExplicitColumnsAttribute), true).Length > 0;
				Columns = new Dictionary<string, PocoColumn>(StringComparer.OrdinalIgnoreCase);

                foreach (var pi in t.GetProperties())
				{
					// Work out if properties is to be included
					var ColAttrs = pi.GetCustomAttributes(typeof(ColumnAttribute), true);
					if (ExplicitColumns)
					{
						if (ColAttrs.Length == 0)
							continue;
					}
					else
					{
						if (pi.GetCustomAttributes(typeof(IgnoreAttribute), true).Length != 0)
							continue;
					}

					var pc = new PocoColumn();
					pc.PropertyInfo = pi;

					// Work out the DB column name
					if (ColAttrs.Length > 0)
					{
						var colattr = (ColumnAttribute)ColAttrs[0];
						pc.ColumnName = colattr.Name;
						if ((colattr as ResultColumnAttribute) != null)
							pc.ResultColumn = true;
					}
					if (pc.ColumnName == null)
					{
						pc.ColumnName = pi.Name;
						if (Database.Mapper != null && !Database.Mapper.MapPropertyToColumn(t, pi, ref pc.ColumnName, ref pc.ResultColumn))
							continue;
					}

					// Store it
					Columns.Add(pc.ColumnName, pc);
				}

				// Build column list for automatic select
				QueryColumns = (from c in Columns where !c.Value.ResultColumn select c.Key).ToArray();

			}

			static bool IsIntegralType(Type t)
			{
				var tc = Type.GetTypeCode(t);
				return tc >= TypeCode.SByte && tc <= TypeCode.UInt64;
			}



			// Create factory function that can convert a IDataReader record into a POCO
			public Delegate GetFactory(string sql, string connString, bool ForceDateTimesToUtc, int firstColumn, int countColumns, IDataReader r)
			{

                //TODO: It would be nice to remove the irrelevant SQL parts - for a mapping operation anything after the SELECT clause isn't required.
                // This would ensure less duplicate entries that get cached, currently both of these queries would be cached even though they are
                // returning the same structured data:
                // SELECT * FROM MyTable ORDER BY MyColumn
                // SELECT * FROM MyTable ORDER BY MyColumn DESC

			    string key;
			    if (UseLongKeys)
			    {
                    key = string.Format("{0}:{1}:{2}:{3}:{4}", sql, connString, ForceDateTimesToUtc, firstColumn, countColumns);
			    }
			    else
			    {
                    //Create a hashed key, we don't want to store so much string data in memory
                    var combiner = new HashCodeCombiner();
                    combiner.AddCaseInsensitiveString(sql);
                    combiner.AddCaseInsensitiveString(connString);
                    combiner.AddObject(ForceDateTimesToUtc);
                    combiner.AddInt(firstColumn);
                    combiner.AddInt(countColumns);
                    key = combiner.GetCombinedHashCode();
			    }


			    var objectCache = _managedCache.GetCache();

			    Func<Delegate> factory = () =>
			    {
                    // Create the method
                    var m = new DynamicMethod("petapoco_factory_" + objectCache.GetCount(), type, new Type[] { typeof(IDataReader) }, true);
                    var il = m.GetILGenerator();

#if !PETAPOCO_NO_DYNAMIC
                    if (type == typeof(object))
                    {
                        // var poco=new T()
                        il.Emit(OpCodes.Newobj, typeof(System.Dynamic.ExpandoObject).GetConstructor(Type.EmptyTypes));			// obj

                        MethodInfo fnAdd = typeof(IDictionary<string, object>).GetMethod("Add");

                        // Enumerate all fields generating a set assignment for the column
                        for (int i = firstColumn; i < firstColumn + countColumns; i++)
                        {
                            var srcType = r.GetFieldType(i);

                            il.Emit(OpCodes.Dup);						// obj, obj
                            il.Emit(OpCodes.Ldstr, r.GetName(i));		// obj, obj, fieldname

                            // Get the converter
                            Func<object, object> converter = null;
                            if (Database.Mapper != null)
                                converter = Database.Mapper.GetFromDbConverter(null, srcType);
                            if (ForceDateTimesToUtc && converter == null && srcType == typeof(DateTime))
                                converter = delegate(object src) { return new DateTime(((DateTime)src).Ticks, DateTimeKind.Utc); };

                            // Setup stack for call to converter
                            AddConverterToStack(il, converter);

                            // r[i]
                            il.Emit(OpCodes.Ldarg_0);					// obj, obj, fieldname, converter?,    rdr
                            il.Emit(OpCodes.Ldc_I4, i);					// obj, obj, fieldname, converter?,  rdr,i
                            il.Emit(OpCodes.Callvirt, fnGetValue);		// obj, obj, fieldname, converter?,  value

                            // Convert DBNull to null
                            il.Emit(OpCodes.Dup);						// obj, obj, fieldname, converter?,  value, value
                            il.Emit(OpCodes.Isinst, typeof(DBNull));	// obj, obj, fieldname, converter?,  value, (value or null)
                            var lblNotNull = il.DefineLabel();
                            il.Emit(OpCodes.Brfalse_S, lblNotNull);		// obj, obj, fieldname, converter?,  value
                            il.Emit(OpCodes.Pop);						// obj, obj, fieldname, converter?
                            if (converter != null)
                                il.Emit(OpCodes.Pop);					// obj, obj, fieldname,
                            il.Emit(OpCodes.Ldnull);					// obj, obj, fieldname, null
                            if (converter != null)
                            {
                                var lblReady = il.DefineLabel();
                                il.Emit(OpCodes.Br_S, lblReady);
                                il.MarkLabel(lblNotNull);
                                il.Emit(OpCodes.Callvirt, fnInvoke);
                                il.MarkLabel(lblReady);
                            }
                            else
                            {
                                il.MarkLabel(lblNotNull);
                            }

                            il.Emit(OpCodes.Callvirt, fnAdd);
                        }
                    }
                    else
#endif
                        if (type.IsValueType || type == typeof(string) || type == typeof(byte[]))
                        {
                            // Do we need to install a converter?
                            var srcType = r.GetFieldType(0);
                            var converter = GetConverter(ForceDateTimesToUtc, null, srcType, type);

                            // "if (!rdr.IsDBNull(i))"
                            il.Emit(OpCodes.Ldarg_0);										// rdr
                            il.Emit(OpCodes.Ldc_I4_0);										// rdr,0
                            il.Emit(OpCodes.Callvirt, fnIsDBNull);							// bool
                            var lblCont = il.DefineLabel();
                            il.Emit(OpCodes.Brfalse_S, lblCont);
                            il.Emit(OpCodes.Ldnull);										// null
                            var lblFin = il.DefineLabel();
                            il.Emit(OpCodes.Br_S, lblFin);

                            il.MarkLabel(lblCont);

                            // Setup stack for call to converter
                            AddConverterToStack(il, converter);

                            il.Emit(OpCodes.Ldarg_0);										// rdr
                            il.Emit(OpCodes.Ldc_I4_0);										// rdr,0
                            il.Emit(OpCodes.Callvirt, fnGetValue);							// value

                            // Call the converter
                            if (converter != null)
                                il.Emit(OpCodes.Callvirt, fnInvoke);

                            il.MarkLabel(lblFin);
                            il.Emit(OpCodes.Unbox_Any, type);								// value converted
                        }
                        else
                        {
                            // var poco=new T()
                            il.Emit(OpCodes.Newobj, type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null));

                            // Enumerate all fields generating a set assignment for the column
                            for (int i = firstColumn; i < firstColumn + countColumns; i++)
                            {
                                // Get the PocoColumn for this db column, ignore if not known
                                PocoColumn pc;
                                if (!Columns.TryGetValue(r.GetName(i), out pc))
                                    continue;

                                // Get the source type for this column
                                var srcType = r.GetFieldType(i);
                                var dstType = pc.PropertyInfo.PropertyType;

                                // "if (!rdr.IsDBNull(i))"
                                il.Emit(OpCodes.Ldarg_0);										// poco,rdr
                                il.Emit(OpCodes.Ldc_I4, i);										// poco,rdr,i
                                il.Emit(OpCodes.Callvirt, fnIsDBNull);							// poco,bool
                                var lblNext = il.DefineLabel();
                                il.Emit(OpCodes.Brtrue_S, lblNext);								// poco

                                il.Emit(OpCodes.Dup);											// poco,poco

                                // Do we need to install a converter?
                                var converter = GetConverter(ForceDateTimesToUtc, pc, srcType, dstType);

                                // Fast
                                bool Handled = false;
                                if (converter == null)
                                {
                                    var valuegetter = typeof(IDataRecord).GetMethod("Get" + srcType.Name, new Type[] { typeof(int) });
                                    if (valuegetter != null
                                            && valuegetter.ReturnType == srcType
                                            && (valuegetter.ReturnType == dstType || valuegetter.ReturnType == Nullable.GetUnderlyingType(dstType)))
                                    {
                                        il.Emit(OpCodes.Ldarg_0);										// *,rdr
                                        il.Emit(OpCodes.Ldc_I4, i);										// *,rdr,i
                                        il.Emit(OpCodes.Callvirt, valuegetter);							// *,value

                                        // Convert to Nullable
                                        if (Nullable.GetUnderlyingType(dstType) != null)
                                        {
                                            il.Emit(OpCodes.Newobj, dstType.GetConstructor(new Type[] { Nullable.GetUnderlyingType(dstType) }));
                                        }

                                        il.Emit(OpCodes.Callvirt, pc.PropertyInfo.GetSetMethod(true));		// poco
                                        Handled = true;
                                    }
                                }

                                // Not so fast
                                if (!Handled)
                                {
                                    // Setup stack for call to converter
                                    AddConverterToStack(il, converter);

                                    // "value = rdr.GetValue(i)"
                                    il.Emit(OpCodes.Ldarg_0);										// *,rdr
                                    il.Emit(OpCodes.Ldc_I4, i);										// *,rdr,i
                                    il.Emit(OpCodes.Callvirt, fnGetValue);							// *,value

                                    // Call the converter
                                    if (converter != null)
                                        il.Emit(OpCodes.Callvirt, fnInvoke);

                                    // Assign it
                                    il.Emit(OpCodes.Unbox_Any, pc.PropertyInfo.PropertyType);		// poco,poco,value
                                    il.Emit(OpCodes.Callvirt, pc.PropertyInfo.GetSetMethod(true));		// poco
                                }

                                il.MarkLabel(lblNext);
                            }

                            var fnOnLoaded = RecurseInheritedTypes<MethodInfo>(type, (x) => x.GetMethod("OnLoaded", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[0], null));
                            if (fnOnLoaded != null)
                            {
                                il.Emit(OpCodes.Dup);
                                il.Emit(OpCodes.Callvirt, fnOnLoaded);
                            }
                        }

                    il.Emit(OpCodes.Ret);

                    // return it
                    var del = m.CreateDelegate(Expression.GetFuncType(typeof(IDataReader), type));

                    return del;
			    };

                //lazy usage of AddOrGetExisting ref: http://stackoverflow.com/questions/10559279/how-to-deal-with-costly-building-operations-using-memorycache/15894928#15894928
                var newValue = new Lazy<Delegate>(factory);
                // the line belows returns existing item or adds the new value if it doesn't exist
                var value = (Lazy<Delegate>)objectCache.AddOrGetExisting(key, newValue, new CacheItemPolicy
                {
                    //sliding expiration of 1 hr, if the same key isn't used in this
                    // timeframe it will be removed from the cache
                    SlidingExpiration = new TimeSpan(0, 0, SlidingExpirationSeconds)
                });
                return (value ?? newValue).Value; // Lazy<T> handles the locking itself

			}

			private static void AddConverterToStack(ILGenerator il, Func<object, object> converter)
			{
				if (converter != null)
				{
					// Add the converter
					int converterIndex = m_Converters.Count;
					m_Converters.Add(converter);

					// Generate IL to push the converter onto the stack
					il.Emit(OpCodes.Ldsfld, fldConverters);
					il.Emit(OpCodes.Ldc_I4, converterIndex);
					il.Emit(OpCodes.Callvirt, fnListGetItem);					// Converter
				}
			}

			private static Func<object, object> GetConverter(bool forceDateTimesToUtc, PocoColumn pc, Type srcType, Type dstType)
			{
				Func<object, object> converter = null;

				// Get converter from the mapper
				if (Database.Mapper != null)
				{
					if (pc != null)
					{
						converter = Database.Mapper.GetFromDbConverter(pc.PropertyInfo, srcType);
					}
					else
					{
						var m2 = Database.Mapper as IMapper2;
						if (m2 != null)
						{
							converter = m2.GetFromDbConverter(dstType, srcType);
						}
					}
				}

				// Standard DateTime->Utc mapper
				if (forceDateTimesToUtc && converter == null && srcType == typeof(DateTime) && (dstType == typeof(DateTime) || dstType == typeof(DateTime?)))
				{
					converter = delegate(object src) { return new DateTime(((DateTime)src).Ticks, DateTimeKind.Utc); };
				}

				// Forced type conversion including integral types -> enum
				if (converter == null)
				{
					if (dstType.IsEnum && IsIntegralType(srcType))
					{
						if (srcType != typeof(int))
						{
							converter = delegate(object src) { return Convert.ChangeType(src, typeof(int), null); };
						}
					}
					else if (!dstType.IsAssignableFrom(srcType))
					{
						converter = delegate(object src) { return Convert.ChangeType(src, dstType, null); };
					}
				}
				return converter;
			}


			static T RecurseInheritedTypes<T>(Type t, Func<Type, T> cb)
			{
				while (t != null)
				{
					T info = cb(t);
					if (info != null)
						return info;
					t = t.BaseType;
				}
				return default(T);
			}

            ManagedCache _managedCache = new ManagedCache();
			static Dictionary<Type, PocoData> m_PocoDatas = new Dictionary<Type, PocoData>();
			static List<Func<object, object>> m_Converters = new List<Func<object, object>>();
			static MethodInfo fnGetValue = typeof(IDataRecord).GetMethod("GetValue", new Type[] { typeof(int) });
			static MethodInfo fnIsDBNull = typeof(IDataRecord).GetMethod("IsDBNull");
			static FieldInfo fldConverters = typeof(PocoData).GetField("m_Converters", BindingFlags.Static | BindingFlags.GetField | BindingFlags.NonPublic);
			static MethodInfo fnListGetItem = typeof(List<Func<object, object>>).GetProperty("Item").GetGetMethod();
			static MethodInfo fnInvoke = typeof(Func<object, object>).GetMethod("Invoke");
			public Type type;
			public string[] QueryColumns { get; private set; }
			public TableInfo TableInfo { get; private set; }
			public Dictionary<string, PocoColumn> Columns { get; private set; }
            static System.Threading.ReaderWriterLockSlim InnerLock = new System.Threading.ReaderWriterLockSlim();

            /// <summary>
            /// Returns a report of the current cache being utilized by PetaPoco
            /// </summary>
            /// <returns></returns>
		    public static string PrintDebugCacheReport(out double totalBytes, out IEnumerable<string> allKeys)
            {
                var managedCache = new ManagedCache();

                var sb = new StringBuilder();
                sb.AppendLine("m_PocoDatas:");
                foreach (var pocoData in m_PocoDatas)
                {
                    sb.AppendFormat("\t{0}\n", pocoData.Key);
                    sb.AppendFormat("\t\tTable:{0} - Col count:{1}\n", pocoData.Value.TableInfo.TableName, pocoData.Value.QueryColumns.Length);
                }

                var cache = managedCache.GetCache();
                allKeys = cache.Select(x => x.Key).ToArray();

                sb.AppendFormat("\tTotal Poco data count:{0}\n", allKeys.Count());

                var keys = string.Join("", cache.Select(x => x.Key));
                //Bytes in .Net are stored as utf-16 = unicode little endian
                totalBytes = Encoding.Unicode.GetByteCount(keys);

                sb.AppendFormat("\tTotal byte for keys:{0}\n", totalBytes);

                sb.AppendLine("\tAll Poco cache items:");

                foreach (var item in cache)
                {
                    sb.AppendFormat("\t\t Key -> {0}\n", item.Key);
                    sb.AppendFormat("\t\t Value -> {0}\n", item.Value);
                }

                sb.AppendLine("-------------------END REPORT------------------------");
                return sb.ToString();
            }
		}


		// Member variables
	    readonly string _connectionString;
	    readonly string _providerName;
		DbProviderFactory _factory;
		IDbConnection _sharedConnection;
		IDbTransaction _transaction;
		int _sharedConnectionDepth;
		int _transactionDepth;
		bool _transactionCancelled;
		string _lastSql;
		object[] _lastArgs;
		string _paramPrefix = "@";
	    IsolationLevel _isolationLevel;
	}

	// Transaction object helps maintain transaction depth counts
	public class Transaction : IDisposable
	{
        public Transaction(Database db, IsolationLevel isolationLevel)
        {
            _db = db;
            _db.BeginTransaction(isolationLevel);
        }

		public virtual void Complete()
		{
			_db.CompleteTransaction();
			_db = null;
		}

		public void Dispose()
		{
            //TODO prevent multiple calls to Dispose
			if (_db != null)
				_db.AbortTransaction();
		}

		Database _db;
	}

	// Simple helper class for building SQL statments
	public class Sql
	{
		public Sql()
		{
		}

		public Sql(string sql, params object[] args)
		{
			_sql = sql;
			_args = args;
		}

		public static Sql Builder
		{
			get { return new Sql(); }
		}

		string _sql;
		object[] _args;
		Sql _rhs;
		string _sqlFinal;
		object[] _argsFinal;

		private void Build()
		{
			// already built?
			if (_sqlFinal != null)
				return;

			// Build it
			var sb = new StringBuilder();
			var args = new List<object>();
			Build(sb, args, null);
			_sqlFinal = sb.ToString();
			_argsFinal = args.ToArray();
		}

		public string SQL
		{
			get
			{
				Build();
				return _sqlFinal;
			}
		}

		public object[] Arguments
		{
			get
			{
				Build();
				return _argsFinal;
			}
		}

		public Sql Append(Sql sql)
		{
			if (_rhs != null)
				_rhs.Append(sql);
			else
				_rhs = sql;

			_sqlFinal = null;
			return this;
		}

		public Sql Append(string sql, params object[] args)
		{
			return Append(new Sql(sql, args));
		}

		static bool Is(Sql sql, string sqltype)
		{
			return sql != null && sql._sql != null && sql._sql.StartsWith(sqltype, StringComparison.InvariantCultureIgnoreCase);
		}

		private void Build(StringBuilder sb, List<object> args, Sql lhs)
		{
			if (!String.IsNullOrEmpty(_sql))
			{
				// Add SQL to the string
				if (sb.Length > 0)
				{
					sb.Append("\n");
				}

				var sql = Database.ProcessParams(_sql, _args, args);

				if (Is(lhs, "WHERE ") && Is(this, "WHERE "))
					sql = "AND " + sql.Substring(6);
				if (Is(lhs, "ORDER BY ") && Is(this, "ORDER BY "))
					sql = ", " + sql.Substring(9);

				sb.Append(sql);
			}

			// Now do rhs
			if (_rhs != null)
				_rhs.Build(sb, args, this);
        }

        public Sql Where(string sql, params object[] args)
		{
			return Append(new Sql("WHERE (" + sql + ")", args));
		}

		public Sql OrderBy(params object[] columns)
		{
			return Append(new Sql("ORDER BY " + String.Join(", ", (from x in columns select x.ToString()).ToArray())));
		}

		public Sql Select(params object[] columns)
		{
			return Append(new Sql("SELECT " + String.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        public Sql AndSelect(params object[] columns)
        {
            return Append(new Sql(", " + String.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        public Sql From(params object[] tables)
		{
			return Append(new Sql("FROM " + String.Join(", ", (from x in tables select x.ToString()).ToArray())));
		}

		public Sql GroupBy(params object[] columns)
		{
			return Append(new Sql("GROUP BY " + String.Join(", ", (from x in columns select x.ToString()).ToArray())));
		}

		private SqlJoinClause Join(string JoinType, string table)
		{
			return new SqlJoinClause(Append(new Sql(JoinType + table)));
		}

		public SqlJoinClause InnerJoin(string table) { return Join("INNER JOIN ", table); }
		public SqlJoinClause LeftJoin(string table) { return Join("LEFT JOIN ", table); }
        public SqlJoinClause LeftOuterJoin(string table) { return Join("LEFT OUTER JOIN ", table); }
        public SqlJoinClause RightJoin(string table) { return Join("RIGHT JOIN ", table); }

		public class SqlJoinClause
		{
			private readonly Sql _sql;

			public SqlJoinClause(Sql sql)
			{
				_sql = sql;
			}

			public Sql On(string onClause, params object[] args)
			{
				return _sql.Append("ON " + onClause, args);
			}
		}
	}
}
