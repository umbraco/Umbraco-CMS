using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Querying
{
    public class QueryFactory
    {
        public ISqlSyntaxProvider SqlSyntax { get; private set; }
        public IMappingResolver MappingResolver { get; private set; }

        public QueryFactory(ISqlSyntaxProvider sqlSyntax, IMappingResolver mappingResolver)
        {
            SqlSyntax = sqlSyntax;
            MappingResolver = mappingResolver;
        }

        public Query<T> Create<T>()
        {
            return new Query<T>(SqlSyntax, MappingResolver);
        }
    }
}