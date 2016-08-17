using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Querying
{
    public class QueryFactory : IQueryFactory
    {
        public ISqlSyntaxProvider SqlSyntax { get; }
        public IMapperCollection Mappers { get; }

        public QueryFactory(ISqlSyntaxProvider sqlSyntax, IMapperCollection mappers)
        {
            SqlSyntax = sqlSyntax;
            Mappers = mappers;
        }

        public IQuery<T> Create<T>()
        {
            return new Query<T>(SqlSyntax, Mappers);
        }
    }
}