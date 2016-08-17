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
    internal class ModelToSqlExpressionHelper<T> : BaseExpressionHelper<T>
    {
        private readonly BaseMapper _mapper;

        public ModelToSqlExpressionHelper(ISqlSyntaxProvider sqlSyntax, BaseMapper mapper)
            : base(sqlSyntax)
        {
            if (mapper == null) throw new ArgumentNullException("mapper");
            _mapper = mapper;
        }

        public ModelToSqlExpressionHelper(ISqlSyntaxProvider sqlSyntax, IMappingResolver mappingResolver)
            : base(sqlSyntax)
        {
            _mapper = mappingResolver.ResolveMapperByType(typeof(T));

            if (_mapper == null)
            {
                throw new InvalidOperationException("Could not resolve a mapper for type " + typeof (T));
            }
        }

        protected override string VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null &&
                m.Expression.NodeType == ExpressionType.Parameter
                && m.Expression.Type == typeof(T))
            {
                var field = _mapper.Map(SqlSyntax, m.Member.Name, true);
                if (field.IsNullOrWhiteSpace())
                    throw new InvalidOperationException("The mapper returned an empty field for the member name: " + m.Member.Name);
                return field;
            }

            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Convert)
            {
                var field = _mapper.Map(SqlSyntax, m.Member.Name, true);
                if (field.IsNullOrWhiteSpace())
                    throw new InvalidOperationException("The mapper returned an empty field for the member name: " + m.Member.Name);
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

        //protected bool IsFieldName(string quotedExp)
        //{
        //    //Not entirely sure this is reliable, but its better then simply returning true
        //    return quotedExp.LastIndexOf("'", StringComparison.InvariantCultureIgnoreCase) + 1 != quotedExp.Length;
        //}
    }
}