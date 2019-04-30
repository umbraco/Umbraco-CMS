﻿using System;
using System.Linq.Expressions;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Persistence.Querying
{
    /// <summary>
    /// An expression tree parser to create SQL statements and SQL parameters based on a strongly typed expression,
    /// based on Umbraco's business logic models.
    /// </summary>
    /// <remarks>This object is stateful and cannot be re-used to parse an expression.</remarks>
    internal class ModelToSqlExpressionVisitor<T> : ExpressionVisitorBase
    {
        private readonly IMapperCollection _mappers;
        private readonly BaseMapper _mapper;

        public ModelToSqlExpressionVisitor(ISqlSyntaxProvider sqlSyntax, IMapperCollection mappers)
            : base(sqlSyntax)
        {
            _mappers = mappers;
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
                    var field = _mapper.Map(m.Member.Name);
                    if (field.IsNullOrWhiteSpace())
                        throw new InvalidOperationException($"The mapper returned an empty field for the member name: {m.Member.Name} for type: {m.Expression.Type}.");
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
                    var field = _mapper.Map(m.Member.Name);
                    if (field.IsNullOrWhiteSpace())
                        throw new InvalidOperationException($"The mapper returned an empty field for the member name: {m.Member.Name} for type: {m.Expression.Type}.");
                    return field;
                }

                //already compiled, return
                return string.Empty;
            }

            if (m.Expression != null
                && m.Expression.Type != typeof(T)
                && TypeHelper.IsTypeAssignableFrom<IUmbracoEntity>(m.Expression.Type) //TODO: Could this just be `IEntity` ? why does it need to be IUmbracoEntity, we aren't even using the reference to that below
                && EndsWithConstant(m) == false)
            {
                //if this is the case, it means we have a sub expression / nested property access, such as: x.ContentType.Alias == "Test";
                //and since the sub type (x.ContentType) is not the same as x, we need to resolve a mapper for x.ContentType to get it's mapped SQL column

                //don't execute if compiled
                if (Visited == false)
                {
                    var subMapper = _mappers[m.Expression.Type]; // throws if not found
                    var field = subMapper.Map(m.Member.Name);
                    if (field.IsNullOrWhiteSpace())
                        throw new InvalidOperationException($"The mapper returned an empty field for the member name: {m.Member.Name} for type: {m.Expression.Type}");
                    return field;
                }
                //already compiled, return
                return string.Empty;
            }

            // TODO: When m.Expression.NodeType == ExpressionType.Constant and it's an expression like: content => aliases.Contains(content.ContentType.Alias);
            // then an SQL parameter will be added for aliases as an array, however in SqlIn on the subclass it will manually add these SqlParameters anyways,
            // however the query will still execute because the SQL that is written will only contain the correct indexes of SQL parameters, this would be ignored,
            // I'm just unsure right now due to time constraints how to make it correct. It won't matter right now and has been working already with this bug but I've
            // only just discovered what it is actually doing.

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

        /// <summary>
        /// Determines if the MemberExpression ends in a Constant value
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        private static bool EndsWithConstant(MemberExpression m)
        {
            Expression expr = m;

            while (expr is MemberExpression)
            {
                var memberExpr = expr as MemberExpression;
                expr = memberExpr.Expression;
            }

            var constExpr = expr as ConstantExpression;
            return constExpr != null;
        }
    }
}
