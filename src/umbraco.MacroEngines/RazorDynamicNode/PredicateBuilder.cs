using System.Linq.Expressions;

namespace System.Linq.Dynamic
{
	[Obsolete("This class is superceded by Umbraco.Core.ExpressionExtensions")]
	public static class PredicateBuilder
	{
		public static Expression<Func<T, bool>> True<T>()
		{
			return Umbraco.Core.ExpressionExtensions.True<T>();
		}
		public static Expression<Func<T, bool>> False<T>()
		{
			return Umbraco.Core.ExpressionExtensions.False<T>();
		}

		public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
		                                              Expression<Func<T, bool>> expr2)
		{
			return Umbraco.Core.ExpressionExtensions.Or<T>(expr1, expr2);
		}

		public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
		                                               Expression<Func<T, bool>> expr2)
		{
			return Umbraco.Core.ExpressionExtensions.And<T>(expr1, expr2);
		}
	}
}