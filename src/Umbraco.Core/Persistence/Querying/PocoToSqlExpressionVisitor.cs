using System;
using System.Linq;
using System.Linq.Expressions;
using NPoco;

namespace Umbraco.Core.Persistence.Querying
{
    /// <summary>
    /// Represents an expression tree parser used to turn strongly typed expressions into SQL statements.
    /// </summary>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <remarks>This visitor is stateful and cannot be reused.</remarks>
    internal class PocoToSqlExpressionVisitor<TDto> : ExpressionVisitorBase
    {
        private readonly PocoData _pd;

        public PocoToSqlExpressionVisitor(ISqlContext sqlContext)
            : base(sqlContext.SqlSyntax)
        {
            _pd = sqlContext.PocoDataFactory.ForType(typeof(TDto));
        }

        protected override string VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter && m.Expression.Type == typeof(TDto))
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

    /// <summary>
    /// Represents an expression tree parser used to turn strongly typed expressions into SQL statements.
    /// </summary>
    /// <typeparam name="TDto1">The type of DTO 1.</typeparam>
    /// <typeparam name="TDto2">The type of DTO 2.</typeparam>
    /// <remarks>This visitor is stateful and cannot be reused.</remarks>
    internal class PocoToSqlExpressionVisitor<TDto1, TDto2> : ExpressionVisitorBase
    {
        private readonly PocoData _pocoData1, _pocoData2;
        private readonly string _alias1, _alias2;
        private string _parameterName1, _parameterName2;

        public PocoToSqlExpressionVisitor(ISqlContext sqlContext, string alias1, string alias2)
            : base(sqlContext.SqlSyntax)
        {
            _pocoData1 = sqlContext.PocoDataFactory.ForType(typeof (TDto1));
            _pocoData2 = sqlContext.PocoDataFactory.ForType(typeof (TDto2));
            _alias1 = alias1;
            _alias2 = alias2;
        }

        protected override string VisitLambda(LambdaExpression lambda)
        {
            if (lambda.Parameters.Count == 2)
            {
                _parameterName1 = lambda.Parameters[0].Name;
                _parameterName2 = lambda.Parameters[1].Name;
            }
            return base.VisitLambda(lambda);
        }

        protected override string VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null)
            {
                if (m.Expression.NodeType == ExpressionType.Parameter)
                {
                    var pex = (ParameterExpression) m.Expression;

                    if (pex.Name == _parameterName1)
                        return Visited ? string.Empty : GetFieldName(_pocoData1, m.Member.Name, _alias1);

                    if (pex.Name == _parameterName2)
                        return Visited ? string.Empty : GetFieldName(_pocoData2, m.Member.Name, _alias2);
                }
                else if (m.Expression.NodeType == ExpressionType.Convert)
                {
                    // here: which _pd should we use?!
                    throw new NotSupportedException();
                    //return Visited ? string.Empty : GetFieldName(_pd, m.Member.Name);
                }
            }

            var member = Expression.Convert(m, typeof (object));
            var lambda = Expression.Lambda<Func<object>>(member);
            var getter = lambda.Compile();
            var o = getter();

            SqlParameters.Add(o);

            // execute if not already compiled
            return Visited ? string.Empty : $"@{SqlParameters.Count - 1}";
        }

        protected virtual string GetFieldName(PocoData pocoData, string name, string alias)
        {
            var column = pocoData.Columns.FirstOrDefault(x => x.Value.MemberInfoData.Name == name);
            var tableName = SqlSyntax.GetQuotedTableName(alias ?? pocoData.TableInfo.TableName);
            var columnName = SqlSyntax.GetQuotedColumnName(column.Value.ColumnName);

            return tableName + "." + columnName;
        }
    }

}
