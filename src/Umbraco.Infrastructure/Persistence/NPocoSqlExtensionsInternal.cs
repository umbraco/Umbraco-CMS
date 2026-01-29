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
    public static class NPocoSqlExtensionsInternal
    {
        internal static string GetAliasedField(this Sql<ISqlContext> sql, string field)
        {
            // get alias, if aliased
            //
            // regex looks for pattern "([\w+].[\w+]) AS ([\w+])" ie "(field) AS (alias)"
            // and, if found & a group's field matches the field name, returns the alias
            //
            // so... if query contains "[umbracoNode].[nodeId] AS [umbracoNode__nodeId]"
            // then GetAliased for "[umbracoNode].[nodeId]" returns "[umbracoNode__nodeId]"

            MatchCollection matches = sql.SqlContext.SqlSyntax.AliasRegex.Matches(sql.SQL);
            Match? match = matches.Cast<Match>().FirstOrDefault(m => m.Groups[1].Value.InvariantEquals(field));
            return match == null ? field : match.Groups[2].Value;
        }

        internal static string GetColumnName(this PropertyInfo column)
        {
            ColumnAttribute? attr = column.FirstAttribute<ColumnAttribute>();
            return string.IsNullOrWhiteSpace(attr?.Name) ? column.Name : attr.Name;
        }

        internal static string[] GetColumns<TDto>(this Sql<ISqlContext> sql, string? tableAlias = null, string? referenceName = null, Expression<Func<TDto, object?>>[]? columnExpressions = null, bool withAlias = true, bool forInsert = false)
        {
            PocoData? pd = sql.SqlContext.PocoDataFactory.ForType(typeof(TDto));
            var tableName = tableAlias ?? pd.TableInfo.TableName;
            var queryColumns = pd.QueryColumns.ToList();

            Dictionary<string, string>? aliases = null;

            if (columnExpressions != null && columnExpressions.Length > 0)
            {
                var names = columnExpressions.Select(x =>
                {
                    (MemberInfo member, var alias) = ExpressionHelper.FindProperty(x);
                    var field = member as PropertyInfo;
                    var fieldName = field?.GetColumnName();
                    if (alias != null && fieldName is not null)
                    {
                        aliases ??= new Dictionary<string, string>();
                        aliases[fieldName] = alias;
                    }
                    return fieldName;
                }).ToArray();

                //only get the columns that exist in the selected names
                queryColumns = queryColumns.Where(x => names.Contains(x.Key)).ToList();

                //ensure the order of the columns in the expressions is the order in the result
                queryColumns.Sort((a, b) => names.IndexOf(a.Key).CompareTo(names.IndexOf(b.Key)));
            }

            string? GetAliasOld(PocoColumn column)
            {
                if (aliases != null && aliases.TryGetValue(column.ColumnName, out var alias))
                {
                    return alias;
                }

                return withAlias ? (string.IsNullOrEmpty(column.ColumnAlias) ? column.MemberInfoKey : column.ColumnAlias) : null;
            }


            return queryColumns
                .Select(x => sql.SqlContext.SqlSyntax.GetColumn(
                    sql.SqlContext.DatabaseType,
                    tableName,
                    x.Value.ColumnName,
                    GetAlias(x.Value, withAlias, aliases)!, // GetAliasOld(x.Value),
                    referenceName,
                    forInsert: forInsert))
                .ToArray();
        }

        private static string? GetAlias(PocoColumn column, bool withAlias = true, Dictionary<string, string>? aliases = null)
        {
            if (aliases != null && aliases.TryGetValue(column.ColumnName, out var alias))
            {
                return alias;
            }

            // MyTODOs: why does PostgreSQL have issues with these aliases?

            var columnMemberInfoKeyIsUniqueId = column.MemberInfoKey.InvariantEquals("uniqueid") && !column.MemberInfoKey.Equals("uniqueId");
            var columnMemberInfoKeyIsLanguageId = column.MemberInfoKey.InvariantEquals("languageid") && !column.MemberInfoKey.Equals("languageId");

            if (columnMemberInfoKeyIsUniqueId || columnMemberInfoKeyIsLanguageId)
            {
                var fallbackAlias = string.IsNullOrEmpty(column.ColumnAlias)
                        ? column.ColumnName
                        : column.ColumnAlias;

                return withAlias
                    ? fallbackAlias
                    : null;
            }

            return withAlias ? (string.IsNullOrEmpty(column.ColumnAlias) ? column.MemberInfoKey : column.ColumnAlias) : null;
        }        
    }
}
