using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using StackExchange.Profiling;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

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
            EnableSqlTrace = false;
        }

        public UmbracoDatabase(string connectionString, string providerName, ILogger logger)
            : base(connectionString, providerName)
        {
            _logger = logger;
            EnableSqlTrace = false;
        }

        public UmbracoDatabase(string connectionString, DbProviderFactory provider, ILogger logger)
            : base(connectionString, provider)
        {
            _logger = logger;
            EnableSqlTrace = false;
        }

        public UmbracoDatabase(string connectionStringName, ILogger logger)
            : base(connectionStringName)
        {
            _logger = logger;
            EnableSqlTrace = false;
        }

        public override IDbConnection OnConnectionOpened(IDbConnection connection)
        {
            // propagate timeout if none yet

            // wrap the connection with a profiling connection that tracks timings
            return new StackExchange.Profiling.Data.ProfiledDbConnection(connection as DbConnection, MiniProfiler.Current);
        }

        public override void OnException(Exception x)
        {
            _logger.Error<UmbracoDatabase>("Database exception occurred", x);
            base.OnException(x);
        }

        public override void OnExecutingCommand(IDbCommand cmd)
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

        public override void OnExecutedCommand(IDbCommand cmd)
        {
            if (_enableCount)
            {
                SqlCount++;
            }
            base.OnExecutedCommand(cmd);
        }

        /// <summary>
        /// We are overriding this in the case that we are using SQL Server 2012+ so we can make paging more efficient than the default PetaPoco paging
        /// see: http://issues.umbraco.org/issue/U4-8837
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="sqlSelectRemoved"></param>
        /// <param name="sqlOrderBy"></param>
        /// <param name="args"></param>
        /// <param name="sqlPage"></param>
        /// <param name="databaseType"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        internal override void BuildSqlDbSpecificPagingQuery(DBType databaseType, long skip, long take, string sql, string sqlSelectRemoved, string sqlOrderBy, ref object[] args, out string sqlPage)
        {
            if (databaseType == DBType.SqlServer)
            {
                //we need to check it's version to see what kind of paging format we can use
                //TODO: This is a hack, but we don't have access to the SqlSyntaxProvider here, we can in v8 but not now otherwise
                // this would be a breaking change.
                var sqlServerSyntax = SqlSyntaxContext.SqlSyntaxProvider as SqlServerSyntaxProvider;
                if (sqlServerSyntax != null)
                {
                    if ((int) sqlServerSyntax.GetVersionName(this) >= (int) SqlServerVersionName.V2012)
                    {
                        //we can use the good paging! to do that we are going to change the databaseType to SQLCE since
                        //it also uses the good paging syntax.
                        base.BuildSqlDbSpecificPagingQuery(DBType.SqlServerCE, skip, take, sql, sqlSelectRemoved, sqlOrderBy, ref args, out sqlPage);
                        return;
                    }
                }
            }
           
            //use the defaults
            base.BuildSqlDbSpecificPagingQuery(databaseType, skip, take, sql, sqlSelectRemoved, sqlOrderBy, ref args, out sqlPage);
        }
    }
}