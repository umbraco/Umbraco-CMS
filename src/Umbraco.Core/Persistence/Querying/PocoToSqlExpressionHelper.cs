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
    internal class PocoToSqlExpressionHelper<T> : BaseExpressionHelper<T>
    {
        private readonly Database.PocoData _pd;

        public PocoToSqlExpressionHelper()
        {
            _pd = new Database.PocoData(typeof(T));
        }
        
        protected override string VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null &&
               m.Expression.NodeType == ExpressionType.Parameter
               && m.Expression.Type == typeof(T))
            {
                string field = GetFieldName(_pd, m.Member.Name);
                return field;
            }

            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Convert)
            {
                string field = GetFieldName(_pd, m.Member.Name);
                return field;
            }

            var member = Expression.Convert(m, typeof(object));
            var lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            object o = getter();

            SqlParameters.Add(o);
            return string.Format("@{0}", SqlParameters.Count - 1);

            //return GetQuotedValue(o, o != null ? o.GetType() : null);

        }
        
        protected virtual string GetFieldName(Database.PocoData pocoData, string name)
        {
            var column = pocoData.Columns.FirstOrDefault(x => x.Value.PropertyInfo.Name == name);
            return string.Format("{0}.{1}",
                SqlSyntaxContext.SqlSyntaxProvider.GetQuotedTableName(pocoData.TableInfo.TableName),
                SqlSyntaxContext.SqlSyntaxProvider.GetQuotedColumnName(column.Value.ColumnName));
        }
        
        //protected bool IsFieldName(string quotedExp)
        //{
        //    return true;
        //}
    }
}