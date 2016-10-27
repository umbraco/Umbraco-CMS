using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Querying
{
    /// <summary>
    /// An expression tree parser to create SQL statements and SQL parameters based on a given strongly typed expression based on Umbraco's business logic Models
    /// </summary>
    /// <remarks>
    /// This object stores state, it cannot be re-used to parse an expression
    /// </remarks>
    internal class ModelToSqlExpressionHelper<T> : BaseExpressionHelper
    {

        private readonly BaseMapper _mapper;

        public ModelToSqlExpressionHelper(ISqlSyntaxProvider sqlSyntax, BaseMapper mapper) : base(sqlSyntax)
        {
            _mapper = mapper;
        }

        public ModelToSqlExpressionHelper() : this(SqlSyntaxContext.SqlSyntaxProvider, MappingResolver.Current.ResolveMapperByType(typeof(T)))
        {
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
                    var field = _mapper.Map(m.Member.Name, true);
                    if (field.IsNullOrWhiteSpace())
                        throw new InvalidOperationException("The mapper returned an empty field for the member name: " + m.Member.Name);
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
                    var field = _mapper.Map(m.Member.Name, true);
                    if (field.IsNullOrWhiteSpace())
                        throw new InvalidOperationException("The mapper returned an empty field for the member name: " + m.Member.Name);
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
    }
}