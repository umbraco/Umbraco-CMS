using System;
using System.Linq;
using System.Linq.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Querying
{
    /// <summary>
    /// An expression tree parser to create SQL statements and SQL parameters based on a strongly typed expression,
    /// based on Umbraco's DTOs.
    /// </summary>
    /// <remarks>This object is stateful and cannot be re-used to parse an expression.</remarks>
    internal class PocoToSqlExpressionVisitor<T> : ExpressionVisitorBase
    {
        private readonly Database.PocoData _pd;


        public PocoToSqlExpressionVisitor(ISqlSyntaxProvider syntaxProvider)
            : base(syntaxProvider)
        {
            _pd = new Database.PocoData(typeof(T));
        }

        [Obsolete("Use the overload the specifies a SqlSyntaxProvider")]
        public PocoToSqlExpressionVisitor()
            : this(SqlSyntaxContext.SqlSyntaxProvider)
        {            
        }

        protected override string VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null &&
               m.Expression.NodeType == ExpressionType.Parameter
               && m.Expression.Type == typeof(T))
            {
                //don't execute if compiled
                if (Visited == false)
                {
                    string field = GetFieldName(_pd, m.Member.Name);
                    return field;
                }
                //already compiled, return
                return string.Empty;
            }

            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Convert)
            {
                //don't execute if compiled
                if (Visited == false)
                {
                    string field = GetFieldName(_pd, m.Member.Name);
                    return field;
                }
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
                return string.Format("@{0}", SqlParameters.Count - 1);
            //already compiled, return
            return string.Empty;
        }

        protected virtual string GetFieldName(Database.PocoData pocoData, string name)
        {
            var column = pocoData.Columns.FirstOrDefault(x => x.Value.PropertyInfo.Name == name);
            return string.Format("{0}.{1}",
                SqlSyntax.GetQuotedTableName(pocoData.TableInfo.TableName),
                SqlSyntax.GetQuotedColumnName(column.Value.ColumnName));
        }
    }
}