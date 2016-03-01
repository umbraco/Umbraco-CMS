using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NPoco;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Querying
{
    internal class PocoToSqlExpressionHelper<T> : BaseExpressionHelper<T>
    {
        private readonly PocoData _pd;

        public PocoToSqlExpressionHelper(ISqlSyntaxProvider sqlSyntaxProvider)
            : base(sqlSyntaxProvider)
        {
            // fixme
            // not how it works - should go through the PocoDataFactory + should cache
            //_pd = new PocoData(typeof(T));
            var z = new PocoDataFactory(new PocoMapper());
            _pd = z.ForType(typeof (T));
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

        protected virtual string GetFieldName(PocoData pocoData, string name)
        {
            var column = pocoData.Columns.FirstOrDefault(x => x.Value.MemberInfo.Name == name);
            return string.Format("{0}.{1}",
                SqlSyntax.GetQuotedTableName(pocoData.TableInfo.TableName),
                SqlSyntax.GetQuotedColumnName(column.Value.ColumnName));
        }

        //protected bool IsFieldName(string quotedExp)
        //{
        //    return true;
        //}
    }
}