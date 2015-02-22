using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Querying
{
    public class QueryFactory
    {
        private readonly ISqlSyntaxProvider _sqlSyntax;
        private readonly IMappingResolver _mappingResolver;

        public QueryFactory(ISqlSyntaxProvider sqlSyntax, IMappingResolver mappingResolver)
        {
            _sqlSyntax = sqlSyntax;
            _mappingResolver = mappingResolver;
        }

        public Query<T> Create<T>()
        {
            return new Query<T>(_sqlSyntax, _mappingResolver);
        }
    }
}