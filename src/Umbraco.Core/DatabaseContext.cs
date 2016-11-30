using System;
using NPoco;
using Umbraco.Core.DI;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core
{
    /// <summary>
    /// Represents the Umbraco database context.
    /// </summary>
    /// <remarks>
    /// <para>The database context creates Sql statements and IQuery expressions.</para>
    /// <para>The database context provides the SqlSyntax and the DatabaseType.</para>
    /// <para>The database context provides access to the "ambient" database.</para>
    /// <para>The database context provides basic status infos (whether the db is configured and can connect).</para>
    /// </remarks>
    public class DatabaseContext
    {
        private readonly IDatabaseFactory _databaseFactory;
        private bool _canConnectOnce;

        // fixme
        // do we need to expose the query factory here?
        // all in all, would prob. mean replacing ALL repository.Query by something more meaningful? YES!

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContext"/> class.
        /// </summary>
        /// <param name="databaseFactory">A database factory.</param>
        /// <remarks>The database factory will try to configure itself but may fail eg if the default
        /// Umbraco connection string is not available because we are installing. In which case this
        /// database builder must sort things out and configure the database factory before it can be
        /// used.</remarks>
        public DatabaseContext(IDatabaseFactory databaseFactory)
        {
            if (databaseFactory == null) throw new ArgumentNullException(nameof(databaseFactory));

            _databaseFactory = databaseFactory;
        }

        /// <summary>
        /// Gets the query factory.
        /// </summary>
        /// <remarks>In most cases... this is useless, better use Query{T}.</remarks>
        public IQueryFactory QueryFactory => _databaseFactory.QueryFactory;

        /// <summary>
        /// Gets the database sql syntax.
        /// </summary>
        public ISqlSyntaxProvider SqlSyntax => _databaseFactory.SqlSyntax;

        /// <summary>
        /// Creates a Sql statement.
        /// </summary>
        public Sql<SqlContext> Sql() => _databaseFactory.Sql();

        /// <summary>
        /// Creates a Sql statement.
        /// </summary>
        public Sql<SqlContext> Sql(string sql, params object[] args) => Sql().Append(sql, args);

        /// <summary>
        /// Creates a Query expression.
        /// </summary>
        public IQuery<T> Query<T>() => _databaseFactory.QueryFactory.Create<T>();

        /// <summary>
        /// Gets an "ambient" database for doing CRUD operations against custom tables that resides in the Umbraco database.
        /// </summary>
        /// <remarks>Should not be used for operation against standard Umbraco tables; as services should be used instead.</remarks>
        public UmbracoDatabase Database => _databaseFactory.GetDatabase();

        /// <summary>
        /// Gets a value indicating whether the database is configured.
        /// </summary>
        /// <remarks>It does not necessarily mean that it is possible to
        /// connect, nor that Umbraco is installed, nor up-to-date.</remarks>
        public bool IsDatabaseConfigured => _databaseFactory.Configured;

        /// <summary>
        /// Gets a value indicating whether it is possible to connect to the database.
        /// </summary>
        public bool CanConnect
        {
            get
            {
                var canConnect = _databaseFactory.Configured  && _databaseFactory.CanConnect;

                if (_canConnectOnce)
                {
                    Current.Logger.Debug<DatabaseContext>("CanConnect: " + canConnect);
                }
                else
                {
                    Current.Logger.Info<DatabaseContext>("CanConnect: " + canConnect);
                    _canConnectOnce = canConnect; // keep logging Info until we can connect
                }

                return canConnect;
            }
        }
    }
}