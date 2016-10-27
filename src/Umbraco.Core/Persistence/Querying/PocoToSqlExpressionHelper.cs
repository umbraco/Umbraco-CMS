using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Querying
{

    /// <summary>
    /// An expression tree parser to create SQL statements and SQL parameters based on a given strongly typed expression based on Umbraco's PetaPoco dto Models
    /// </summary>
    /// <remarks>
    /// This object stores state, it cannot be re-used to parse an expression
    /// </remarks>
    internal class PocoToSqlExpressionHelper<T> : BaseExpressionHelper
    {
        private readonly Database.PocoData _pd;

        public PocoToSqlExpressionHelper() : base(SqlSyntaxContext.SqlSyntaxProvider)
        {
            _pd = new Database.PocoData(typeof(T));
        }
        
        protected override string VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null &&
               m.Expression.NodeType == ExpressionType.Parameter
               && m.Expression.Type == typeof(T))
            {
                //don't execute if compiled
                if (IsCompiled == false)
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
                if (IsCompiled == false)
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
            object o = getter();

            SqlParameters.Add(o);

            //don't execute if compiled
            if (IsCompiled == false)
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