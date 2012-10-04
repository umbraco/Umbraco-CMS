using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Linq.Dynamic
{

	[Obsolete("This class has been superceded by Umbraco.Core.Dynamics.DynamicExpression")]
	public static class DynamicExpression
	{
		public static bool ConvertDynamicNullToBooleanFalse
		{
			get { return Umbraco.Web.Dynamics.DynamicExpression.ConvertDynamicNullToBooleanFalse; }
			set { Umbraco.Web.Dynamics.DynamicExpression.ConvertDynamicNullToBooleanFalse = value; }
		}
		public static Expression Parse(Type resultType, string expression, bool convertDynamicNullToBooleanFalse, params object[] values)
		{
			return Umbraco.Web.Dynamics.DynamicExpression.Parse(resultType, expression, convertDynamicNullToBooleanFalse, values);
		}

		public static LambdaExpression ParseLambda(Type itType, Type resultType, string expression, bool convertDynamicNullToBooleanFalse, params object[] values)
		{
			return Umbraco.Web.Dynamics.DynamicExpression.ParseLambda(itType, resultType, expression, convertDynamicNullToBooleanFalse, values);
		}

		public static LambdaExpression ParseLambda(ParameterExpression[] parameters, Type resultType, string expression, bool convertDynamicNullToBooleanFalse, params object[] values)
		{
			return Umbraco.Web.Dynamics.DynamicExpression.ParseLambda(parameters, resultType, expression, convertDynamicNullToBooleanFalse, values);
		}

		public static Expression<Func<T, S>> ParseLambda<T, S>(string expression, bool convertDynamicNullToBooleanFalse, params object[] values)
		{
			return Umbraco.Web.Dynamics.DynamicExpression.ParseLambda<T, S>(expression, convertDynamicNullToBooleanFalse, values);
		}

		public static Type CreateClass(params DynamicProperty[] properties)
		{
			return Umbraco.Web.Dynamics.DynamicExpression.CreateClass(properties.Select(x => new Umbraco.Core.Dynamics.DynamicProperty(x.Name, x.Type)));
		}

		public static Type CreateClass(IEnumerable<DynamicProperty> properties)
		{
			return Umbraco.Web.Dynamics.DynamicExpression.CreateClass(properties.Select(x => new Umbraco.Core.Dynamics.DynamicProperty(x.Name, x.Type)));
		}
	}
}