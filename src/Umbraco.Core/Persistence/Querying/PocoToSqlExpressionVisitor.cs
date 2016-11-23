using System;
using System.Linq;
using System.Linq.Expressions;
using NPoco;

namespace Umbraco.Core.Persistence.Querying
{
    /// <summary>
    /// An expression tree parser to create SQL statements and SQL parameters based on a strongly typed expression,
    /// based on Umbraco's DTOs.
    /// </summary>
    /// <remarks>This object is stateful and cannot be re-used to parse an expression.</remarks>
    internal class PocoToSqlExpressionVisitor<T> : ExpressionVisitorBase
    {
        private readonly PocoData _pd;

        public PocoToSqlExpressionVisitor(SqlContext sqlContext)
            : base(sqlContext.SqlSyntax)
        {
            _pd = sqlContext.PocoDataFactory.ForType(typeof(T));
        }

        protected override string VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter && m.Expression.Type == typeof(T))
            {
                //don't execute if compiled
                if (Visited == false)
                    return GetFieldName(_pd, m.Member.Name);

                //already compiled, return
                return string.Empty;
            }

            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Convert)
            {
                //don't execute if compiled
                if (Visited == false)
                    return GetFieldName(_pd, m.Member.Name);

                //already compiled, return
                return string.Empty;
            }

            var member = Expression.Convert(m, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            var o = getter();

            SqlParameters.Add(o);

            //don't execute if compiled
            if (Visited == false)
                return $"@{SqlParameters.Count - 1}";

            //already compiled, return
            return string.Empty;
        }

        protected virtual string GetFieldName(PocoData pocoData, string name)
        {
            var column = pocoData.Columns.FirstOrDefault(x => x.Value.MemberInfoData.Name == name);
            var tableName = SqlSyntax.GetQuotedTableName(pocoData.TableInfo.TableName);
            var columnName = SqlSyntax.GetQuotedColumnName(column.Value.ColumnName);

            return $"{tableName}.{columnName}";
        }
    }
}