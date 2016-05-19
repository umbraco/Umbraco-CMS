using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Querying
{
    public interface IQueryFactory
    {
        IMappingResolver MappingResolver { get; }
        ISqlSyntaxProvider SqlSyntax { get; }
        IQuery<T> Create<T>();
    }
}