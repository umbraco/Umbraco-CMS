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
            if (sqlSyntax == null) throw new ArgumentNullException(nameof(sqlSyntax));
            if (pocoDataFactory == null) throw new ArgumentNullException(nameof(pocoDataFactory));
            if (databaseType == null) throw new ArgumentNullException(nameof(databaseType));
            
            // for tests
            if (mappers == null) mappers = new Mappers.MapperCollection(Enumerable.Empty<BaseMapper>());

            SqlSyntax = sqlSyntax;
            PocoDataFactory = pocoDataFactory;
            DatabaseType = databaseType;
            Mappers = mappers;
        }

        public ISqlSyntaxProvider SqlSyntax { get; }

        public IPocoDataFactory PocoDataFactory { get; }

        public DatabaseType DatabaseType { get; }

        public IMapperCollection Mappers { get; }
    }
}
