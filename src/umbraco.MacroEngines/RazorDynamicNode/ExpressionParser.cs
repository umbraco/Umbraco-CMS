using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using umbraco.MacroEngines;

namespace System.Linq.Dynamic
{
	[Obsolete("This class is no longer used, use Umbraco.Web.Dynamics.ExpressionParser<T> instead")]
	internal class ExpressionParser : Umbraco.Web.Dynamics.ExpressionParser<DynamicNode>
	{
		public ExpressionParser(ParameterExpression[] parameters, string expression, object[] values, bool flagConvertDynamicNullToBooleanFalse)
			: base(parameters, expression, values, flagConvertDynamicNullToBooleanFalse)
		{
		}
	}


}