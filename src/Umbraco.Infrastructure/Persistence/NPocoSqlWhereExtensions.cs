using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Extensions
{
    public static partial class NPocoSqlExtensions
    {
        public static Sql<ISqlContext> WhereParam<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field, string param)
        {
            string s = $"{sql.GetColumns(columnExpressions: [field], withAlias: false).FirstOrDefault()} = {param}";
            return sql.Where(s, []);
        }
    }
}
