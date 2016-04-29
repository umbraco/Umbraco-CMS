using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NPoco;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations
{
    internal class MigrationContext : IMigrationContext
    {
        public MigrationContext(UmbracoDatabase database, ILogger logger)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            Expressions = new Collection<IMigrationExpression>();
            Database = database;
            Logger = logger;
        }

        public ICollection<IMigrationExpression> Expressions { get; set; }

        public UmbracoDatabase Database { get; }

        public ISqlSyntaxProvider SqlSyntax => Database.SqlSyntax;

        public DatabaseType DatabaseType => Database.DatabaseType;

        public ILogger Logger { get; }
    }
}