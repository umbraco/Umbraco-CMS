using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using NPoco;
using StackExchange.Profiling;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.FaultHandling;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Extends NPoco Database for Umbraco.
    /// </summary>
    /// <remarks>
    /// <para>Is used everywhere in place of the original NPoco Database object, and provides additional features
    /// such as profiling, retry policies, logging, etc.</para>
    /// <para>Is never created directly but obtained from the <see cref="DefaultDatabaseFactory"/>.</para>
    /// </remarks>
    public class UmbracoDatabase : Database, IDisposeOnRequestEnd
    {
        // Umbraco's default isolation level is RepeatableRead
        private const IsolationLevel DefaultIsolationLevel = IsolationLevel.RepeatableRead;

        private readonly ILogger _logger;
        private readonly Guid _instanceId = Guid.NewGuid();
        private readonly RetryPolicy _connectionRetryPolicy;
        private readonly RetryPolicy _commandRetryPolicy;
        private bool _enableCount;

        /// <summary>
        /// Used for testing
        /// </summary>
        internal Guid InstanceId
        {
            get { return _instanceId; }
        }

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
        public UmbracoDatabase(string connectionString, string providerName, ILogger logger, RetryPolicy connectionRetryPolicy = null, RetryPolicy commandRetryPolicy = null)
            : base(connectionString, providerName, DefaultIsolationLevel)
        {
            _logger = logger;
            _connectionRetryPolicy = connectionRetryPolicy;
            _commandRetryPolicy = commandRetryPolicy;
            EnableSqlTrace = false;
        }

        // used by DefaultDatabaseFactory
        // creates one instance per request
        public UmbracoDatabase(string connectionStringName, ILogger logger, RetryPolicy connectionRetryPolicy = null, RetryPolicy commandRetryPolicy = null)
            : base(connectionStringName, DefaultIsolationLevel)
        {
            _logger = logger;
            _connectionRetryPolicy = connectionRetryPolicy;
            _commandRetryPolicy = commandRetryPolicy;
            EnableSqlTrace = false;
        }

        protected override DbConnection OnConnectionOpened(DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

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
            base.OnExecutingCommand(cmd);
        }

        protected override void OnExecutedCommand(DbCommand cmd)
        {
            if (EnableSqlTrace)
            {
                _logger.Debug<UmbracoDatabase>(cmd.CommandText);
            }
            if (_enableCount)
            {
                SqlCount++;
            }
            base.OnExecutedCommand(cmd);
        }

        public IEnumerable<TResult> FetchByGroups<TResult, TSource>(IEnumerable<TSource> source, int groupSize, Func<IEnumerable<TSource>, Sql<SqlContext>> sqlFactory)
        {
            return source.SelectByGroups(x => Fetch<TResult>(sqlFactory(x)), groupSize);
        }
    }
}