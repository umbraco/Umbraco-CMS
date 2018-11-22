//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;
using Umbraco.Core.Dynamics;
using Umbraco.Web.Models;

namespace Umbraco.Web.Dynamics
{
    //TODO: Much of this can move to Umbraco.Core.Dynamics - but somehow need to remove all the hard coded references to things like 
    // dynamicnull, etc...
    //NOTE:  The OrderBy stuff here seems to be a bit hacked with hard references to umbraco node objects so don't think it can be
    // re-used which is why we have the OrderBy stuff that hasn't been hacked in teh Umbraco.Core.Dynamics

    internal static class DynamicQueryable
    {
        public static IQueryable<T> Where<T>(this IQueryable<T> source, string predicate, params object[] values)
        {
            return (IQueryable<T>)Where<T>((IQueryable)source, predicate, values);
        }

        public static IQueryable Where<T>(this IQueryable source, string predicate, params object[] values)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (predicate == null) throw new ArgumentNullException("predicate");
            LambdaExpression lambda = DynamicExpression.ParseLambda<T>(source.ElementType, typeof(bool), predicate, true, values);
            if (lambda.Parameters.Count > 0 && lambda.Parameters[0].Type == typeof(T))
            {
                //source list is DynamicNode and the lambda returns a Func<object>
                IQueryable<T> typedSource = source as IQueryable<T>;
                var compiledFunc = lambda.Compile();
                Func<T, object> func = null;
                Func<T, bool> boolFunc = null;
                if (compiledFunc is Func<T, object>)
                {
                    func = (Func<T, object>)compiledFunc;
                }
                if (compiledFunc is Func<T, bool>)
                {
                    boolFunc = (Func<T, bool>)compiledFunc;
                }
                return typedSource.Where(delegate(T node)
                {
                    object value = -1;
                    //value = func(node);
                    //I can't figure out why this is double func<>'d
                    try
                    {
                        if (func != null)
                        {
                            var firstFuncResult = func(node);
                            if (firstFuncResult is Func<T, object>)
                            {
                                value = (firstFuncResult as Func<T, object>)(node);
                            }
                            if (firstFuncResult is Func<T, bool>)
                            {
                                value = (firstFuncResult as Func<T, bool>)(node);
                            }
                            if (firstFuncResult is bool)
                            {
                                return (bool)firstFuncResult;
                            }
                            if (value is bool)
                            {
                                return (bool)value;
                            }
                        }
                        if (boolFunc != null)
                        {
                            return boolFunc(node);
                        }
                        return false;
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                        return false;
                    }
                }).AsQueryable();
            }
            else
            {
                return source.Provider.CreateQuery(
                    Expression.Call(
                        typeof(Queryable), "Where",
                        new Type[] { source.ElementType },
                        source.Expression, Expression.Quote(lambda)));
            }
        }

        public static IQueryable Select<T>(this IQueryable<T> source, string selector, params object[] values)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            LambdaExpression lambda = DynamicExpression.ParseLambda<T>(source.ElementType, typeof(object), selector, false, values);
			if (lambda.Parameters.Count > 0 && lambda.Parameters[0].Type == typeof(T))
            {
                //source list is DynamicNode and the lambda returns a Func<object>
				IQueryable<T> typedSource = source as IQueryable<T>;
                var compiledFunc = lambda.Compile();
				Func<T, object> func = null;
				if (compiledFunc is Func<T, object>)
                {
					func = (Func<T, object>)compiledFunc;
                }
				return typedSource.Select(delegate(T node)
                {
                    object value = null;
                    value = func(node);
					if (value is Func<T, object>)
                    {
						var innerValue = (value as Func<T, object>)(node);
                        return innerValue;
                    }
                    return value;
                }).AsQueryable();
            }
            else
            {
                return source.Provider.CreateQuery(
                    Expression.Call(
                        typeof(Queryable), "Select",
                        new Type[] { source.ElementType, lambda.Body.Type },
                        source.Expression, Expression.Quote(lambda)));
            }
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string ordering, Func<Type> getDynamicListTypeCallback, params object[] values)
        {
			return (IQueryable<T>)OrderBy<T>((IQueryable)source, ordering, getDynamicListTypeCallback, values);
        }

		public static IQueryable OrderBy<T>(this IQueryable source, string ordering, Func<Type> getDynamicListTypeCallback, params object[] values)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (ordering == null) throw new ArgumentNullException("ordering");

			IQueryable<T> typedSource = source as IQueryable<T>;
            if (!ordering.Contains(","))
            {
                bool descending = false;
                if (ordering.IndexOf(" descending", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    ordering = ordering.Replace(" descending", "");
                    descending = true;
                }
                if (ordering.IndexOf(" desc", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    ordering = ordering.Replace(" desc", "");
                    descending = true;
                }

                LambdaExpression lambda = DynamicExpression.ParseLambda<T>(source.ElementType, typeof(object), ordering, false, values);
				if (lambda.Parameters.Count > 0 && lambda.Parameters[0].Type == typeof(T))
                {
                    //source list is DynamicNode and the lambda returns a Func<object>
					Func<T, object> func = (Func<T, object>)lambda.Compile();
                    //get the values out
                    var query = typedSource.ToList().ConvertAll(item => new { node = item, key = EvaluateDynamicNodeFunc(item, func) });
                    if (query.Count == 0)
                    {
                        return source;
                    }
                    var types = from i in query
                                group i by i.key.GetType() into g
                                where g.Key != typeof(DynamicNull)
                                orderby g.Count() descending
                                select new { g, Instances = g.Count() };
                    var dominantType = types.First().g.Key;

                    // NH - add culture dependencies
                    StringComparer comp = StringComparer.Create(CultureInfo.CurrentCulture, true);

                    if (!descending)
                    {
                        // if the dominant type is a string we'll ensure that strings are sorted based on culture settings on node
                        if (dominantType.FullName == "System.String") 
                            return query.OrderBy(item => item.key.ToString(), comp).Select(item => item.node).AsQueryable();
                        else
                            return query.OrderBy(item => GetObjectAsTypeOrDefault(item.key, dominantType)).Select(item => item.node).AsQueryable();
                    }
                    else
                    {
                        if (dominantType.FullName == "System.String")
                            return query.OrderByDescending(item => item.key.ToString(), comp).Select(item => item.node).AsQueryable();
                        else
                            return query.OrderByDescending(item => GetObjectAsTypeOrDefault(item.key, dominantType)).Select(item => item.node).AsQueryable();
                    }
                }
            }

            bool isDynamicNodeList = false;
            if (typedSource != null)
            {
                isDynamicNodeList = true;
            }

            ParameterExpression[] parameters = new ParameterExpression[] {
                Expression.Parameter(source.ElementType, "") };
            var parser = new ExpressionParser<T>(parameters, ordering, values, false);
            IEnumerable<DynamicOrdering> orderings = parser.ParseOrdering();
            Expression queryExpr = source.Expression;
            string methodAsc = "OrderBy";
            string methodDesc = "OrderByDescending";
            foreach (DynamicOrdering o in orderings)
            {
                if (!isDynamicNodeList)
                {
                    queryExpr = Expression.Call(
                        typeof(Queryable), o.Ascending ? methodAsc : methodDesc,
                        new Type[] { source.ElementType, o.Selector.Type },
                        queryExpr, Expression.Quote(Expression.Lambda(o.Selector, parameters)));
                }
                else
                {
                    //reroute each stacked Expression.Call into our own methods that know how to deal
                    //with DynamicNode
                    queryExpr = Expression.Call(
							getDynamicListTypeCallback(),    
                            o.Ascending ? methodAsc : methodDesc,
                            null,
                            queryExpr,
                            Expression.Quote(Expression.Lambda(o.Selector, parameters))
                        );
                }
                methodAsc = "ThenBy";
                methodDesc = "ThenByDescending";
            }
            if (isDynamicNodeList)
            {
                return typedSource.Provider.CreateQuery(queryExpr);
            }
            return source.Provider.CreateQuery(queryExpr);

        }
        private static object GetObjectAsTypeOrDefault(object value, Type type)
        {
            if (type.IsAssignableFrom(value.GetType()))
            {
                return (object)Convert.ChangeType(value, type);
            }
            else
            {
                if (type.IsValueType)
                {
                    return Activator.CreateInstance(type);
                }
                return null;
            }
        }
		private static object EvaluateDynamicNodeFunc<T>(T publishedContent, Func<T, object> func)
        {
            object value = -1;
            var firstFuncResult = func(publishedContent);
			if (firstFuncResult is Func<T, object>)
            {
				value = (firstFuncResult as Func<T, object>)(publishedContent);
            }
            if (firstFuncResult.GetType().IsValueType || firstFuncResult is string)
            {
                value = firstFuncResult;
            }
            return value;
        }
        public static IQueryable Take(this IQueryable source, int count)
        {
            if (source == null) throw new ArgumentNullException("source");
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Take",
                    new Type[] { source.ElementType },
                    source.Expression, Expression.Constant(count)));
        }

        public static IQueryable Skip(this IQueryable source, int count)
        {
            if (source == null) throw new ArgumentNullException("source");
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Skip",
                    new Type[] { source.ElementType },
                    source.Expression, Expression.Constant(count)));
        }

        public static IQueryable GroupBy<T>(this IQueryable source, string keySelector, string elementSelector, params object[] values)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (keySelector == null) throw new ArgumentNullException("keySelector");
            if (elementSelector == null) throw new ArgumentNullException("elementSelector");
            LambdaExpression keyLambda = DynamicExpression.ParseLambda<T>(source.ElementType, null, keySelector, true, values);
            LambdaExpression elementLambda = DynamicExpression.ParseLambda<T>(source.ElementType, null, elementSelector, true, values);
            return source.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "GroupBy",
                    new Type[] { source.ElementType, keyLambda.Body.Type, elementLambda.Body.Type },
                    source.Expression, Expression.Quote(keyLambda), Expression.Quote(elementLambda)));
        }

        public static bool Any(this IQueryable source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return (bool)source.Provider.Execute(
                Expression.Call(
                    typeof(Queryable), "Any",
                    new Type[] { source.ElementType }, source.Expression));
        }

        public static int Count(this IQueryable source)
        {
            if (source == null) throw new ArgumentNullException("source");
            return (int)source.Provider.Execute(
                Expression.Call(
                    typeof(Queryable), "Count",
                    new Type[] { source.ElementType }, source.Expression));
        }
    }
}
