using System;
using NPoco;
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
        private readonly IUmbracoDatabaseFactory _databaseFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContext"/> class.
        /// </summary>
        /// <param name="databaseFactory">A database factory.</param>
        /// <remarks>The database factory will try to configure itself but may fail eg if the default
        /// Umbraco connection string is not available because we are installing. In which case this
        /// database builder must sort things out and configure the database factory before it can be
        /// used.</remarks>
        public DatabaseContext(IUmbracoDatabaseFactory databaseFactory)
        {
            if (databaseFactory == null) throw new ArgumentNullException(nameof(databaseFactory));

            _databaseFactory = databaseFactory;
        }

        /// <summary>
        /// Gets the database Sql syntax.
        /// </summary>
        public ISqlSyntaxProvider SqlSyntax => _databaseFactory.SqlSyntax;

        /// <summary>
        /// Creates a Sql statement.
        /// </summary>
        public Sql<SqlContext> Sql() => _databaseFactory.Sql();

        /// <summary>
        /// Creates a Sql statement.
        /// </summary>
        public Sql<SqlContext> Sql(string sql, params object[] args) => _databaseFactory.Sql(sql, args);

        /// <summary>
        /// Creates a Query expression.
        /// </summary>
        public IQuery<T> Query<T>() => _databaseFactory.Query<T>();

        /// <summary>
        /// Gets an ambient database for doing CRUD operations against custom tables that resides in the Umbraco database.
        /// </summary>
        /// <remarks>Should not be used for operation against standard Umbraco tables; as services should be used instead.</remarks>
        public IUmbracoDatabase Database => _databaseFactory.GetDatabase();

        /// <summary>
        /// Gets an ambient database scope.
        /// </summary>
        /// <returns>A disposable object representing the scope.</returns>
        public IDisposable CreateDatabaseScope() // fixme - move over to factory
        {
            return _databaseFactory.CreateScope();
        }

#if DEBUG_DATABASES
        public List<UmbracoDatabase> Databases
        {
            get
            {
                var factory = _databaseFactory as UmbracoDatabaseFactory;
                if (factory == null) throw new NotSupportedException();
                return factory.Databases;
            }
        }
#endif

        /// <summary>
        /// Gets a value indicating whether the database is configured.
        /// </summary>
        /// <remarks>It does not necessarily mean that it is possible to
        /// connect, nor that Umbraco is installed, nor up-to-date.</remarks>
        public bool IsDatabaseConfigured => _databaseFactory.Configured;

        /// <summary>
        /// Gets a value indicating whether it is possible to connect to the database.
        /// </summary>
        public bool CanConnect => _databaseFactory.Configured && _databaseFactory.CanConnect;
    }
}