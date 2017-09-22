using System;
using NPoco;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core
{
    public class DatabaseContext
    {
        private readonly IUmbracoDatabaseFactory _databaseFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="Umbraco.Core.DatabaseContext"/> class.
        /// </summary>
        /// <param name="databaseFactory">A database factory.</param>
        /// <remarks>The database factory will try to configure itself but may fail eg if the default
        /// Umbraco connection string is not available because we are installing. In which case this
        /// database builder must sort things out and configure the database factory before it can be
        /// used.</remarks>
        public DatabaseContext(IUmbracoDatabaseFactory databaseFactory)
        {
            _databaseFactory = databaseFactory ?? throw new ArgumentNullException(nameof(databaseFactory));
        }

        /// <summary>
        /// Gets the database Sql syntax.
        /// </summary>
        public ISqlSyntaxProvider SqlSyntax => _databaseFactory.SqlContext.SqlSyntax;

        /// <summary>
        /// Creates a Sql statement.
        /// </summary>
        public Sql<ISqlContext> Sql() => _databaseFactory.SqlContext.Sql();

        /// <summary>
        /// Creates a Sql statement.
        /// </summary>
        public Sql<ISqlContext> Sql(string sql, params object[] args) => _databaseFactory.SqlContext.Sql(sql, args);

        /// <summary>
        /// Creates a Query expression.
        /// </summary>
        public IQuery<T> Query<T>() => _databaseFactory.SqlContext.Query<T>();

        /// <summary>
        /// Gets an ambient database for doing CRUD operations against custom tables that resides in the Umbraco database.
        /// </summary>
        /// <remarks>Should not be used for operation against standard Umbraco tables; as services should be used instead.</remarks>
        public IUmbracoDatabase Database => throw new NotImplementedException(); // there's no magic?

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
