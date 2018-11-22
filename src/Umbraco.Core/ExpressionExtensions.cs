using System;
using System.Linq;
using System.Linq.Expressions;

namespace Umbraco.Core
{
	internal static class ExpressionExtensions
	{
		public static Expression<Func<T, bool>> True<T>() { return f => true; }
		public static Expression<Func<T, bool>> False<T>() { return f => false; }

		public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left,
		                                              Expression<Func<T, bool>> right)
		{
			var invokedExpr = Expression.Invoke(right, left.Parameters.Cast<Expression>());
			return Expression.Lambda<Func<T, bool>>
				(Expression.OrElse(left.Body, invokedExpr), left.Parameters);
		}

		public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left,
		                                               Expression<Func<T, bool>> right)
		{
			var invokedExpr = Expression.Invoke(right, left.Parameters.Cast<Expression>());
			return Expression.Lambda<Func<T, bool>>
				(Expression.AndAlso(left.Body, invokedExpr), left.Parameters);
		}
	}
}