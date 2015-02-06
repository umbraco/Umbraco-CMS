using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Umbraco.Core.Dynamics
{
    /// <summary>
    /// Extension methods for IQueryable
    /// </summary>
    /// <remarks>
    /// NOTE: Ordering code taken from: http://stackoverflow.com/questions/41244/dynamic-linq-orderby-on-ienumerablet
    /// 
    /// ANOTHER NOTE: We have a bastardized version of Dynamic Linq existing at Umbraco.Web.Dynamics however it's been hacked so not
    /// sure we can use it for anything other than DynamicNode.
    /// </remarks>
    internal static class QueryableExtensions
    {
        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "OrderBy");
        }
        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "OrderByDescending");
        }
        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "ThenBy");
        }
        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property)
        {
            return ApplyOrder<T>(source, property, "ThenByDescending");
        }
        static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName)
        {
            string[] props = property.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (string prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            Type delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            LambdaExpression lambda = Expression.Lambda(delegateType, expr, arg);

            object result = typeof(Queryable).GetMethods().Single(
                method => method.Name == methodName
                          && method.IsGenericMethodDefinition
                          && method.GetGenericArguments().Length == 2
                          && method.GetParameters().Length == 2)
                                             .MakeGenericMethod(typeof(T), type)
                                             .Invoke(null, new object[] { source, lambda });
            return (IOrderedQueryable<T>)result;
        }


    }
}