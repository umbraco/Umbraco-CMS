using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NPoco;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations
{
    internal class MigrationContext : IMigrationContext
    {
        public MigrationContext(IUmbracoDatabase database, ILogger logger)
        {
            Expressions = new Collection<IMigrationExpression>();
            Database = database ?? throw new ArgumentNullException(nameof(database));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ICollection<IMigrationExpression> Expressions { get; set; }

        public IUmbracoDatabase Database { get; }

        public ISqlSyntaxProvider SqlSyntax => Database.SqlSyntax;

        public Sql<SqlContext> Sql() => new Sql<SqlContext>(Database.SqlContext);

        public Sql<SqlContext> Sql(string sql, params object[] args) => new Sql<SqlContext>(Database.SqlContext, sql, args);

        public IQuery<T> Query<T>() => new Query<T>(Database.SqlContext);

        public DatabaseType DatabaseType => Database.DatabaseType;

        public ILogger Logger { get; }

        public ILocalMigration GetLocalMigration() => new LocalMigration(Database, Logger);
    }
}