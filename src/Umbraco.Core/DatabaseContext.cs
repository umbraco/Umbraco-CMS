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
    /// Represents the Umbraco database.
    /// </summary>
    /// <remarks>One per AppDomain. Ensures that the database is available.</remarks>
    public class DatabaseContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContext"/> class.
        /// </summary>
        /// <param name="databaseFactory">A database factory.</param>
        /// <remarks>The database factory will try to configure itself but may fail eg if the default
        /// Umbraco connection string is not available because we are installing. In which case this
        /// database context must sort things out and configure the database factory before it can be
        /// used.</remarks>
        public DatabaseContext(IDatabaseFactory databaseFactory)
        {
            if (databaseFactory == null) throw new ArgumentNullException(nameof(databaseFactory));

            DatabaseFactory = databaseFactory;
        }

        // FIXME
        // this is basically exposing a subset of the database factory...
        // so why? why not "just" merge with database factory?
        // which can create & manage ambient database,
        //           create Sql<SqlContext> expressions
        //           create IQuery<T> expressions
        // ?

        internal IDatabaseFactory DatabaseFactory { get; }

        /// <summary>
        /// Gets the QueryFactory
        /// </summary>
        public IQueryFactory QueryFactory => DatabaseFactory.QueryFactory; // fixme obsolete?

        /// <summary>
        /// Gets the database sql syntax.
        /// </summary>
        public ISqlSyntaxProvider SqlSyntax => DatabaseFactory.SqlSyntax;

        // fixme
        // created by the database factory?
        // add PocoDataFactory
        // add DatabaseType
        // add Sql() and Query<T>()
        // so it can finally replace SqlContext entirely?
        // need an IDatabaseContext interface?

        public Sql<SqlContext> Sql()
        {
            var factory = (DefaultDatabaseFactory) DatabaseFactory; // fixme
            return NPoco.Sql.BuilderFor(factory.SqlContext);
        }

        public Sql<SqlContext> Sql(string sql, params object[] args)
        {
            return Sql().Append(sql, args);
        }

        public IQuery<T> Query<T>()
        {
            return DatabaseFactory.QueryFactory.Create<T>();
        }

        /// <summary>
        /// Gets the <see cref="Database"/> object for doing CRUD operations
        /// against custom tables that resides in the Umbraco database.
        /// </summary>
        /// <remarks>
        /// This should not be used for CRUD operations or queries against the
        /// standard Umbraco tables! Use the Public services for that.
        /// </remarks>
        public UmbracoDatabase Database => DatabaseFactory.GetDatabase();

        /// <summary>
        /// Gets a value indicating whether the database is configured. It does not necessarily
        /// mean that it is possible to connect, nor that Umbraco is installed, nor
        /// up-to-date.
        /// </summary>
        public bool IsDatabaseConfigured => DatabaseFactory.Configured;

        /// <summary>
        /// Gets a value indicating whether it is possible to connect to the database.
        /// </summary>
        public bool CanConnect
        {
            get
            {
                if (DatabaseFactory.Configured == false) return false;
                var canConnect = DatabaseFactory.CanConnect;
                Current.Logger.Info<DatabaseContext>("CanConnect = " + canConnect);
                return canConnect;
            }
        }
    }
}