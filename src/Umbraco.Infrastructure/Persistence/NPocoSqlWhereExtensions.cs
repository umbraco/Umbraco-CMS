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
    public static class NPocoSqlWhereExtensions
    {
        public static Sql<ISqlContext> WhereParam<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field, string param)
        {
            string s = $"{sql.GetColumns(columnExpressions: [field], withAlias: false).FirstOrDefault()} = {param}";
            return sql.Where(s, []);
        }

        /// <summary>
        /// Appends a WHERE IN clause to the Sql statement.
        /// </summary>
        /// <typeparam name="TDto">The type of the Dto.</typeparam>
        /// <param name="sql">The Sql statement.</param>
        /// <param name="field">An expression specifying the field.</param>
        /// <param name="values">The values.</param>
        /// <returns>The Sql statement.</returns>
        public static Sql<ISqlContext> WhereIn<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field, IEnumerable? values)
        {
            if (values == null)
            {
                return sql;
            }

            var fieldName = sql.SqlContext.SqlSyntax.GetFieldName(field);

            string[] stringValues = [.. values.OfType<string>()]; // This is necessary to avoid failing attempting to convert to string[] when values contains non-string types
            if (stringValues.Length > 0)
            {
                Attempt<string[]> attempt = values.TryConvertTo<string[]>();
                if (attempt.Success)
                {
                    values = attempt.Result?.Select(v => v?.ToLower());
                    sql.Where($"LOWER({fieldName}) IN (@values)", new { values });
                    return sql;
                }
            }

            sql.Where($"{fieldName} IN (@values)", new { values });
            return sql;
        }
    }
}
