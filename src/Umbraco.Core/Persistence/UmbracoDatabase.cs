using System;
using System.Data;
using System.Data.Common;
using System.Text;
using NPoco;
using StackExchange.Profiling;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.FaultHandling;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Extends NPoco Database for Umbraco.
    /// </summary>
    /// <remarks>
    /// <para>Is used everywhere in place of the original NPoco Database object, and provides additional features
    /// such as profiling, retry policies, logging, etc.</para>
    /// <para>Is never created directly but obtained from the <see cref="DefaultDatabaseFactory"/>.</para>
    /// <para>It implements IDisposeOnRequestEnd which means it will be disposed when the request ends, which
    /// automatically closes the connection - as implemented by NPoco Database.Dispose().</para>
    /// </remarks>
    public class UmbracoDatabase : Database, IDisposeOnRequestEnd, IUmbracoDatabaseConfig
    {
        // Umbraco's default isolation level is RepeatableRead
        private const IsolationLevel DefaultIsolationLevel = IsolationLevel.RepeatableRead;

        private readonly ILogger _logger;
        private readonly RetryPolicy _connectionRetryPolicy;
        private readonly RetryPolicy _commandRetryPolicy;
        private bool _enableCount;

        /// <summary>
        /// Used for testing
        /// </summary>
        internal Guid InstanceId { get; } = Guid.NewGuid();

        public ISqlSyntaxProvider SqlSyntax { get; }

        /// <summary>
        /// Generally used for testing, will output all SQL statements executed to the logger
        /// </summary>
        internal bool EnableSqlTrace { get; set; }

        /// <summary>
        /// Used for testing
        /// </summary>
        internal void EnableSqlCount()
        {
            _enableCount = true;
        }

        /// <summary>
        /// Used for testing
        /// </summary>
        internal void DisableSqlCount()
        {
            _enableCount = false;
            SqlCount = 0;
        }

        /// <summary>
        /// Used for testing
        /// </summary>
        internal int SqlCount { get; private set; }

        // used by DefaultDatabaseFactory
        // creates one instance per request
        // also used by DatabaseContext for creating DBs and upgrading
        public UmbracoDatabase(string connectionString,
            ISqlSyntaxProvider sqlSyntax, DatabaseType databaseType, DbProviderFactory provider,
            ILogger logger,
            RetryPolicy connectionRetryPolicy = null, RetryPolicy commandRetryPolicy = null)
            : base(connectionString, databaseType, provider, DefaultIsolationLevel)
        {
            SqlSyntax = sqlSyntax;
            _logger = logger;
            _connectionRetryPolicy = connectionRetryPolicy;
            _commandRetryPolicy = commandRetryPolicy;
            EnableSqlTrace = false;
        }

        // INTERNAL FOR UNIT TESTS
        internal UmbracoDatabase(DbConnection connection,
            ISqlSyntaxProvider sqlSyntax, DatabaseType databaseType, DbProviderFactory provider,
            ILogger logger)
            : base(connection, databaseType, provider, DefaultIsolationLevel)
        {
            SqlSyntax = sqlSyntax;
            _logger = logger;
            EnableSqlTrace = false;
        }

        // fixme: that could be an extension method of IUmbracoDatabaseConfig
        public Sql<SqlContext> Sql()
        {
            return NPoco.Sql.BuilderFor(new SqlContext(this));
        }

        protected override DbConnection OnConnectionOpened(DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));

            // wrap the connection with a profiling connection that tracks timings
            connection = new StackExchange.Profiling.Data.ProfiledDbConnection(connection, MiniProfiler.Current);

            // wrap the connection with a retrying connection
            if (_connectionRetryPolicy != null || _commandRetryPolicy != null)
                connection = new RetryDbConnection(connection, _connectionRetryPolicy, _commandRetryPolicy);

            return connection;
        }

        protected override void OnException(Exception x)
        {
            _logger.Error<UmbracoDatabase>("Database exception occurred", x);
            base.OnException(x);
        }

        // fixme.poco - has new interceptors?

        protected override void OnExecutingCommand(DbCommand cmd)
        {
            // if no timeout is specified, and the connection has a longer timeout, use it
            if (OneTimeCommandTimeout == 0 && CommandTimeout == 0 && cmd.Connection.ConnectionTimeout > 30)
                cmd.CommandTimeout = cmd.Connection.ConnectionTimeout;

            if (EnableSqlTrace)
            {
                var sb = new StringBuilder();
                sb.Append(cmd.CommandText);
                foreach (DbParameter p in cmd.Parameters)
                {
                    sb.Append(" - ");
                    sb.Append(p.Value);
                }
                
                _logger.Debug<UmbracoDatabase>(sb.ToString());
            }

            base.OnExecutingCommand(cmd);
        }

        protected override void OnExecutedCommand(DbCommand cmd)
        {
            if (_enableCount)
            {
                SqlCount++;
            }
            base.OnExecutedCommand(cmd);
        }
    }
}