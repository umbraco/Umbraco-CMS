using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using umbraco.MacroEngines;

namespace System.Linq.Dynamic
{
	internal class ExpressionParser : Umbraco.Core.Dynamics.ExpressionParser
	{
		public ExpressionParser(ParameterExpression[] parameters, string expression, object[] values)
			: base(parameters, expression, values)
		{
		}
	}


}