using System;
using System.Linq.Expressions;

namespace Umbraco.Core
{
    internal static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> True<T>() { return f => true; }

        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var invokedExpr = Expression.Invoke(right, left.Parameters);
            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(left.Body, invokedExpr), left.Parameters);
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var invokedExpr = Expression.Invoke(right, left.Parameters);
            return Expression.Lambda<Func<T, bool>> (Expression.AndAlso(left.Body, invokedExpr), left.Parameters);
        }

        public static Action CompileToDelegate(this Expression<Action> expr)
        {
            return ReflectionUtilities.CompileToDelegate(expr);
        }

        public static Action<T1> CompileToDelegate<T1>(this Expression<Action<T1>> expr)
        {
            return ReflectionUtilities.CompileToDelegate(expr);
        }

        public static Action<T1, T2> CompileToDelegate<T1, T2>(this Expression<Action<T1, T2>> expr)
        {
            return ReflectionUtilities.CompileToDelegate(expr);
        }

        public static Action<T1, T2, T3> CompileToDelegate<T1, T2, T3>(this Expression<Action<T1, T2, T3>> expr)
        {
            return ReflectionUtilities.CompileToDelegate(expr);
        }

        public static Func<TResult> CompileToDelegate<TResult>(this Expression<Func<TResult>> expr)
        {
            return ReflectionUtilities.CompileToDelegate(expr);
        }

        public static Func<T1, TResult> CompileToDelegate<T1, TResult>(this Expression<Func<T1, TResult>> expr)
        {
            return ReflectionUtilities.CompileToDelegate(expr);
        }

        public static Func<T1, T2, TResult> CompileToDelegate<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> expr)
        {
            return ReflectionUtilities.CompileToDelegate(expr);
        }

        public static Func<T1, T2, T3, TResult> CompileToDelegate<T1, T2, T3, TResult>(this Expression<Func<T1, T2, T3, TResult>> expr)
        {
            return ReflectionUtilities.CompileToDelegate(expr);
        }

        public static TMethod CompileToDelegate<TMethod>(this Expression<TMethod> expr)
        {
            return ReflectionUtilities.CompileToDelegate(expr);
        }
    }
}
