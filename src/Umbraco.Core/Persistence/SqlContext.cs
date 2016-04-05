using NPoco;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    public class SqlContext
    {
        public SqlContext(ISqlSyntaxProvider sqlSyntax, IPocoDataFactory pocoDataFactory, DatabaseType databaseType)
        {
            SqlSyntax = sqlSyntax;
            PocoDataFactory = pocoDataFactory;
            DatabaseType = databaseType;
        }

        public SqlContext(ISqlSyntaxProvider sqlSyntax, IDatabaseConfig database)
        {
            SqlSyntax = sqlSyntax;
            PocoDataFactory = database.PocoDataFactory;
            DatabaseType = database.DatabaseType;
        }

        public ISqlSyntaxProvider SqlSyntax { get; }

        public IPocoDataFactory PocoDataFactory { get; }

        public DatabaseType DatabaseType { get; }
    }
}
