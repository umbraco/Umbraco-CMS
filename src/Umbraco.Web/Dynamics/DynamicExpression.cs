using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Umbraco.Core.Dynamics;

namespace Umbraco.Web.Dynamics
{
	internal static class DynamicExpression
	{
		//public static bool ConvertDynamicNullToBooleanFalse = false;

		public static Expression Parse<T>(Type resultType, string expression, bool convertDynamicNullToBooleanFalse, params object[] values)
		{
			//ConvertDynamicNullToBooleanFalse = convertDynamicNullToBooleanFalse;
			var parser = new ExpressionParser<T>(null, expression, values, convertDynamicNullToBooleanFalse);
			return parser.Parse(resultType);
		}

		public static LambdaExpression ParseLambda<T>(Type itType, Type resultType, string expression, bool convertDynamicNullToBooleanFalse, params object[] values)
		{
			return ParseLambda<T>(new ParameterExpression[] { Expression.Parameter(itType, "") }, resultType, expression, convertDynamicNullToBooleanFalse, values);
		}

		public static LambdaExpression ParseLambda<T>(ParameterExpression[] parameters, Type resultType, string expression, bool convertDynamicNullToBooleanFalse, params object[] values)
		{
			//ConvertDynamicNullToBooleanFalse = convertDynamicNullToBooleanFalse;
			var parser = new ExpressionParser<T>(parameters, expression, values, convertDynamicNullToBooleanFalse);
			return Expression.Lambda(parser.Parse(resultType), parameters);
		}

		public static Expression<Func<T, S>> ParseLambda<TDoc, T, S>(string expression, bool convertDynamicNullToBooleanFalse, params object[] values)
		{
			return (Expression<Func<T, S>>)ParseLambda<TDoc>(typeof(T), typeof(S), expression, convertDynamicNullToBooleanFalse, values);
		}

		public static Type CreateClass(params DynamicProperty[] properties)
		{
			return ClassFactory.Instance.GetDynamicClass(properties);
		}

		public static Type CreateClass(IEnumerable<DynamicProperty> properties)
		{
			return ClassFactory.Instance.GetDynamicClass(properties);
		}
	}
}