using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using StackExchange.Profiling;
using Umbraco.Core.Logging;

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
        /// <summary>
        /// Used for testing
        /// </summary>
        internal Guid InstanceId
        {
            get { return _instanceId; }
        }

        [Obsolete("Use the other constructor specifying an ILogger instead")]
        public UmbracoDatabase(IDbConnection connection)
            : this(connection, LoggerResolver.Current.Logger)
        {
        }

        [Obsolete("Use the other constructor specifying an ILogger instead")]
        public UmbracoDatabase(string connectionString, string providerName)
            : this(connectionString, providerName, LoggerResolver.Current.Logger)
        {
        }

        [Obsolete("Use the other constructor specifying an ILogger instead")]
        public UmbracoDatabase(string connectionString, DbProviderFactory provider)
            : this(connectionString, provider, LoggerResolver.Current.Logger)
        {
        }

        [Obsolete("Use the other constructor specifying an ILogger instead")]
        public UmbracoDatabase(string connectionStringName)
            : this(connectionStringName, LoggerResolver.Current.Logger)
        {
        }

        public UmbracoDatabase(IDbConnection connection, ILogger logger)
            : base(connection)
        {
            _logger = logger;
        }

        public UmbracoDatabase(string connectionString, string providerName, ILogger logger)
            : base(connectionString, providerName)
        {
            _logger = logger;
        }

        public UmbracoDatabase(string connectionString, DbProviderFactory provider, ILogger logger)
            : base(connectionString, provider)
        {
            _logger = logger;
        }

        public UmbracoDatabase(string connectionStringName, ILogger logger)
            : base(connectionStringName)
        {
            _logger = logger;
        }

        public override IDbConnection OnConnectionOpened(IDbConnection connection)
        {
            // wrap the connection with a profiling connection that tracks timings 
            return new StackExchange.Profiling.Data.ProfiledDbConnection(connection as DbConnection, MiniProfiler.Current);
        }

        public override void OnException(Exception x)
        {
            _logger.Info<UmbracoDatabase>(x.StackTrace);
            base.OnException(x);
        }
    }
}