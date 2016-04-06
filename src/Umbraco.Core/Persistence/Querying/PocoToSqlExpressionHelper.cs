using System;
using System.Linq;
using System.Linq.Expressions;
using NPoco;

namespace Umbraco.Core.Persistence.Querying
{
    internal class PocoToSqlExpressionHelper<T> : BaseExpressionHelper<T>
    {
        private readonly PocoData _pd;

        public PocoToSqlExpressionHelper(SqlContext sqlContext)
            : base(sqlContext.SqlSyntax)
        {
            _pd = sqlContext.PocoDataFactory.ForType(typeof (T));
        }

        protected override string VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter && m.Expression.Type == typeof (T))
            {
                var field = GetFieldName(_pd, m.Member.Name);
                return field;
            }

            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Convert)
            {
                var field = GetFieldName(_pd, m.Member.Name);
                return field;
            }

            var member = Expression.Convert(m, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            var o = getter();

            SqlParameters.Add(o);

            return $"@{SqlParameters.Count - 1}";
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