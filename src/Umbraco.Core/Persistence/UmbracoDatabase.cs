using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using NPoco;
using StackExchange.Profiling;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.FaultHandling;

namespace Umbraco.Core.Persistence
{

    /// <summary>
    /// Extends NPoco Database for Umbraco.
    /// </summary>
    /// <remarks>
    /// <para>Is used everywhere in place of the original NPoco Database object, and provides additional features
    /// such as profiling, retry policies, logging, etc.</para>
    /// <para>Is never created directly but obtained from the <see cref="UmbracoDatabaseFactory"/>.</para>
    /// </remarks>
    public class UmbracoDatabase : Database, IUmbracoDatabase
    {
        private readonly ILogger _logger;
        private readonly RetryPolicy _connectionRetryPolicy;
        private readonly RetryPolicy _commandRetryPolicy;
        private readonly Guid _instanceGuid = Guid.NewGuid();

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoDatabase"/> class.
        /// </summary>
        /// <remarks>
        /// <para>Used by UmbracoDatabaseFactory to create databases.</para>
        /// <para>Also used by DatabaseBuilder for creating databases and installing/upgrading.</para>
        /// </remarks>
        public UmbracoDatabase(string connectionString, ISqlContext sqlContext, DbProviderFactory provider, ILogger logger, RetryPolicy connectionRetryPolicy = null, RetryPolicy commandRetryPolicy = null)
            : base(connectionString, sqlContext.DatabaseType, provider, sqlContext.SqlSyntax.DefaultIsolationLevel)
        {
            SqlContext = sqlContext;
            _logger = logger;
            _connectionRetryPolicy = connectionRetryPolicy;
            _commandRetryPolicy = commandRetryPolicy;
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoDatabase"/> class.
        /// </summary>
        /// <remarks>Internal for unit tests only.</remarks>
        internal UmbracoDatabase(DbConnection connection, ISqlContext sqlContext, ILogger logger)
            : base(connection, sqlContext.DatabaseType, sqlContext.SqlSyntax.DefaultIsolationLevel)
        {
            SqlContext = sqlContext;
            _logger = logger;
            Init();
        }

        private void Init()
        {
            EnableSqlTrace = EnableSqlTraceDefault;
            NPocoDatabaseExtensions.ConfigureNPocoBulkExtensions();
            if (SqlContext.DatabaseType == DatabaseType.SQLCe)
            {
                Mappers.Add(new SqlCeImageMapper());
            }
        }

        #endregion

        /// <inheritdoc />
        public ISqlContext SqlContext { get; }

        #region Temp

        // work around NPoco issue https://github.com/schotime/NPoco/issues/517 while we wait for the fix
        public override DbCommand CreateCommand(DbConnection connection, CommandType commandType, string sql, params object[] args)
        {
            var command = base.CreateCommand(connection, commandType, sql, args);

            if (!DatabaseType.IsSqlCe()) return command;

            foreach (DbParameter parameter in command.Parameters)
                if (parameter.Value == DBNull.Value)
                    parameter.DbType = DbType.String;

            return command;
        }

        #endregion

        #region Testing, Debugging and Troubleshooting

        private bool _enableCount;

#if DEBUG_DATABASES
        private int _spid = -1;
        private const bool EnableSqlTraceDefault = true;
#else
        private string _instanceId;
        private const bool EnableSqlTraceDefault = false;
#endif

        /// <inheritdoc />
        public string InstanceId
        {
            get
            {
#if DEBUG_DATABASES
                return _instanceGuid.ToString("N").Substring(0, 8) + ':' + _spid;
#else
                return _instanceId ?? (_instanceId = _instanceGuid.ToString("N").Substring(0, 8));
#endif
            }
        }

        /// <inheritdoc />
        public bool InTransaction { get; private set; }

        protected override void OnBeginTransaction()
        {
            base.OnBeginTransaction();
            InTransaction = true;
        }

        protected override void OnAbortTransaction()
        {
            InTransaction = false;
            base.OnAbortTransaction();
        }

        protected override void OnCompleteTransaction()
        {
            InTransaction = false;
            base.OnCompleteTransaction();
        }

        /// <summary>
        /// Gets or sets a value indicating whether to log all executed Sql statements.
        /// </summary>
        internal bool EnableSqlTrace { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to count all executed Sql statements.
        /// </summary>
        internal bool EnableSqlCount
        {
            get => _enableCount;
            set
            {
                _enableCount = value;
                if (_enableCount == false)
                    SqlCount = 0;
            }
        }

        /// <summary>
        /// Gets the count of all executed Sql statements.
        /// </summary>
        internal int SqlCount { get; private set; }

        #endregion

        #region OnSomething

        // TODO: has new interceptors to replace OnSomething?

        protected override DbConnection OnConnectionOpened(DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

#if DEBUG_DATABASES
            // determines the database connection SPID for debugging
            if (DatabaseType.IsSqlServer())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT @@SPID";
                    _spid = Convert.ToInt32(command.ExecuteScalar());
                }
            }
            else
            {
                // includes SqlCE
                _spid = 0;
            }
#endif

            // wrap the connection with a profiling connection that tracks timings
            connection = new StackExchange.Profiling.Data.ProfiledDbConnection(connection, MiniProfiler.Current);

            // wrap the connection with a retrying connection
            if (_connectionRetryPolicy != null || _commandRetryPolicy != null)
                connection = new RetryDbConnection(connection, _connectionRetryPolicy, _commandRetryPolicy);

            return connection;
        }

#if DEBUG_DATABASES
        protected override void OnConnectionClosing(DbConnection conn)
        {
            _spid = -1;
            base.OnConnectionClosing(conn);
        }
#endif

        protected override void OnException(Exception ex)
        {
            _logger.Error<UmbracoDatabase, string>(ex, "Exception ({InstanceId}).", InstanceId);
            _logger.Debug<UmbracoDatabase, string>("At:\r\n{StackTrace}", Environment.StackTrace);
            if (EnableSqlTrace == false)
                _logger.Debug<UmbracoDatabase, string>("Sql:\r\n{Sql}", CommandToString(LastSQL, LastArgs));
            base.OnException(ex);
        }

        private DbCommand _cmd;

        protected override void OnExecutingCommand(DbCommand cmd)
        {
            // if no timeout is specified, and the connection has a longer timeout, use it
            if (OneTimeCommandTimeout == 0 && CommandTimeout == 0 && cmd.Connection.ConnectionTimeout > 30)
                cmd.CommandTimeout = cmd.Connection.ConnectionTimeout;

            if (EnableSqlTrace)
                _logger.Debug<UmbracoDatabase, string>("SQL Trace:\r\n{Sql}", CommandToString(cmd).Replace("{", "{{").Replace("}", "}}")); // TODO: these escapes should be builtin

#if DEBUG_DATABASES
            // detects whether the command is already in use (eg still has an open reader...)
            DatabaseDebugHelper.SetCommand(cmd, InstanceId + " [T" + System.Threading.Thread.CurrentThread.ManagedThreadId + "]");
            var refsobj = DatabaseDebugHelper.GetReferencedObjects(cmd.Connection);
            if (refsobj != null) _logger.Debug<UmbracoDatabase>("Oops!" + Environment.NewLine + refsobj);
#endif

            _cmd = cmd;
            base.OnExecutingCommand(cmd);
        }

        private string CommandToString(DbCommand cmd)
        {
            return CommandToString(cmd.CommandText, cmd.Parameters.Cast<DbParameter>().Select(x => x.Value).ToArray());
        }

        private string CommandToString(string sql, object[] args)
        {
            var text = new StringBuilder();
#if DEBUG_DATABASES
                text.Append(InstanceId);
                text.Append(": ");
#endif
            NPocoSqlExtensions.ToText(sql, args, text);
            return text.ToString();
        }

        protected override void OnExecutedCommand(DbCommand cmd)
        {
            if (_enableCount)
                SqlCount++;

            base.OnExecutedCommand(cmd);
        }

        #endregion

    }
}
