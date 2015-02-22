using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations
{
    internal class MigrationContext : IMigrationContext
    {
        public MigrationContext(DatabaseProviders databaseProvider, Database database, ILogger logger, ISqlSyntaxProvider sqlSyntax)
        {
            if (database == null) throw new ArgumentNullException("database");
            if (logger == null) throw new ArgumentNullException("logger");
            if (sqlSyntax == null) throw new ArgumentNullException("sqlSyntax");

            Expressions = new Collection<IMigrationExpression>();
            CurrentDatabaseProvider = databaseProvider;
            Database = database;
            Logger = logger;
            SqlSyntax = sqlSyntax;
        }

        public ICollection<IMigrationExpression> Expressions { get; set; }

        public DatabaseProviders CurrentDatabaseProvider { get; private set; }

        public Database Database { get; private set; }

        public ISqlSyntaxProvider SqlSyntax { get; private set; }

        public ILogger Logger { get; private set; }
    }
}