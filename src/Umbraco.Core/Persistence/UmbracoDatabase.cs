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
    /// Represents the Umbraco implementation of the PetaPoco Database object
    /// </summary>
    /// <remarks>
    /// Currently this object exists for 'future proofing' our implementation. By having our own inheritied implementation we 
    /// can then override any additional execution (such as additional loggging, functionality, etc...) that we need to without breaking compatibility since we'll always be exposing
    /// this object instead of the base PetaPoco database object.	
    /// </remarks>
    public class UmbracoDatabase : Database, IDisposeOnRequestEnd
    {
        private readonly ILogger _logger;
        private readonly Guid _instanceId = Guid.NewGuid();
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

        private void CommonInitialize()
        {
            EnableSqlTrace = false;
            Mapper = new PocoMapper(); // fixme inject!
            IsolationLevel = IsolationLevel.RepeatableRead; // fixme else comes from dbType.GetDefaultTransactionIsolationLevel() and cannot override?
        }

        // not used
        public UmbracoDatabase(IDbConnection connection, ILogger logger)
            : base(connection)
        {
            _logger = logger;
            CommonInitialize();
        }

        // used by DefaultDatabaseFactory
        // creates one instance per request
        // also used by DatabaseContext for creating DBs and upgrading
        public UmbracoDatabase(string connectionString, string providerName, ILogger logger)
            : base(connectionString, providerName)
        {
            _logger = logger;
            CommonInitialize();
        }

        // not used
        public UmbracoDatabase(string connectionString, DbProviderFactory provider, ILogger logger)
            : base(connectionString, provider)
        {
            _logger = logger;
            CommonInitialize();
        }

        // used by DefaultDatabaseFactory
        // creates one instance per request
        public UmbracoDatabase(string connectionStringName, ILogger logger)
            : base(connectionStringName)
        {
            _logger = logger;
            CommonInitialize();
        }

        protected override IDbConnection OnConnectionOpened(IDbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            var con = connection as DbConnection;
            if (con == null) throw new ArgumentException("Not a DbConnection.", "connection");

            // wrap the connection with a profiling connection that tracks timings 
            con = new StackExchange.Profiling.Data.ProfiledDbConnection(con, MiniProfiler.Current);

            // wrap the connection with a retrying connection
            // fixme - inject policies, do not recompute all the time!
            var connectionString = connection.ConnectionString ?? string.Empty;
            var conRetryPolicy = RetryPolicyFactory.GetDefaultSqlConnectionRetryPolicyByConnectionString(connectionString);
            var cmdRetryPolicy = RetryPolicyFactory.GetDefaultSqlCommandRetryPolicyByConnectionString(connectionString);
            if (conRetryPolicy != null || cmdRetryPolicy != null)
                con = new RetryDbConnection(con, conRetryPolicy, cmdRetryPolicy);

            return con;
        }

        protected override void OnException(Exception x)
        {
            _logger.Error<UmbracoDatabase>("Database exception occurred", x);
            base.OnException(x);
        }

        protected override void OnExecutedCommand(IDbCommand cmd)
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
    }
}