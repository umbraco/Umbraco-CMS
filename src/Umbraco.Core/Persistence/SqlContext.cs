using System;
using System.Linq;
using NPoco;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
    public class SqlContext
    {
        public SqlContext(ISqlSyntaxProvider sqlSyntax, IPocoDataFactory pocoDataFactory, DatabaseType databaseType, IMapperCollection mappers = null)
        {
            // for tests
            if (mappers == null) mappers = new Mappers.MapperCollection(Enumerable.Empty<BaseMapper>());
            Mappers = mappers;

            SqlSyntax = sqlSyntax ?? throw new ArgumentNullException(nameof(sqlSyntax));
            PocoDataFactory = pocoDataFactory ?? throw new ArgumentNullException(nameof(pocoDataFactory));
            DatabaseType = databaseType ?? throw new ArgumentNullException(nameof(databaseType));
        }

        public ISqlSyntaxProvider SqlSyntax { get; }

        public IPocoDataFactory PocoDataFactory { get; }

        public DatabaseType DatabaseType { get; }

        public IMapperCollection Mappers { get; }
    }
}
