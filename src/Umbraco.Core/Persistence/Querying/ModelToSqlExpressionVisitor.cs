using System;
using System.Linq.Expressions;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Querying
{
    /// <summary>
    /// An expression tree parser to create SQL statements and SQL parameters based on a strongly typed expression,
    /// based on Umbraco's business logic models.
    /// </summary>
    /// <remarks>This object is stateful and cannot be re-used to parse an expression.</remarks>
    internal class ModelToSqlExpressionVisitor<T> : ExpressionVisitorBase
    {
        private readonly BaseMapper _mapper;

        public ModelToSqlExpressionVisitor(ISqlSyntaxProvider sqlSyntax, BaseMapper mapper)
            : base(sqlSyntax)
        {
            if (mapper == null) throw new ArgumentNullException(nameof(mapper));
            _mapper = mapper;
        }

        public ModelToSqlExpressionVisitor(ISqlSyntaxProvider sqlSyntax, IMapperCollection mappers)
            : base(sqlSyntax)
        {
            _mapper = mappers[typeof(T)]; // throws if not found
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
                    var field = _mapper.Map(SqlSyntax, m.Member.Name, true);
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
                if (Visited == false)
                {
                    var field = _mapper.Map(SqlSyntax, m.Member.Name, true);
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
            var o = getter();

            SqlParameters.Add(o);

            //don't execute if compiled
            if (Visited == false)
                return $"@{SqlParameters.Count - 1}";

            //already compiled, return
            return string.Empty;

        }
    }
}