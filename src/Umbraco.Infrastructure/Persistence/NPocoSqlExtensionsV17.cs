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
    public static class NPocoSqlExtensionsV17
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

        /// <summary>
        /// Adds a SQL SELECT statement to retrieve the maximum value of the specified field from the table associated
        /// with the specified DTO type.
        /// </summary>
        /// <typeparam name="TDto">The type of the Data Transfer Object (DTO) that represents the table from which the maximum value will be
        /// selected.</typeparam>
        /// <param name="sql">The SQL query builder to which the SELECT statement will be appended. Cannot be <see langword="null"/>.</param>
        /// <param name="field">An expression specifying the field for which the maximum value will be calculated. Cannot be <see
        /// langword="null"/>.</param>
        /// <param name="coalesceValue">COALESCE string value.</param>
        /// <returns>A modified SQL query builder that includes the SELECT statement for the maximum value of the specified
        /// field or the coalesceValue.</returns>
        public static Sql<ISqlContext> SelectMax<TDto>(this Sql<ISqlContext> sql, Expression<Func<TDto, object?>> field, string coalesceValue)
        {
            ArgumentNullException.ThrowIfNull(sql);
            ArgumentNullException.ThrowIfNull(field);

            return sql.Select($"COALESCE(MAX {sql.SqlContext.SqlSyntax.GetFieldName(field)}), '{coalesceValue}')");
        }

    }
}
