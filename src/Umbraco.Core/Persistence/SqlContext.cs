using System;
using NPoco;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    public class SqlContext
    {
        public SqlContext(ISqlSyntaxProvider sqlSyntax, IPocoDataFactory pocoDataFactory, DatabaseType databaseType)
        {
            if (sqlSyntax == null) throw new ArgumentNullException(nameof(sqlSyntax));
            if (pocoDataFactory == null) throw new ArgumentNullException(nameof(pocoDataFactory));
            if (databaseType == null) throw new ArgumentNullException(nameof(databaseType));

            SqlSyntax = sqlSyntax;
            PocoDataFactory = pocoDataFactory;
            DatabaseType = databaseType;
        }

        public SqlContext(IUmbracoDatabaseConfig database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            SqlSyntax = database.SqlSyntax;
            PocoDataFactory = database.PocoDataFactory;
            DatabaseType = database.DatabaseType;
        }

        public ISqlSyntaxProvider SqlSyntax { get; }

        public IPocoDataFactory PocoDataFactory { get; }

        public DatabaseType DatabaseType { get; }
    }
}
