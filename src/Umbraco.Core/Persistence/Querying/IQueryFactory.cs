using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Querying
{
    public interface IQueryFactory
    {
        IMapperCollection Mappers { get; }
        ISqlSyntaxProvider SqlSyntax { get; }
        IQuery<T> Create<T>();
    }
}