using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Umbraco.Web.Models;

namespace Umbraco.Web.Dynamics
{
    public static class DynamicPublishedContentListOrdering
    {

        private static TOut Reduce<TOut>(Func<DynamicPublishedContent, TOut> func, DynamicPublishedContent publishedContent)
        {
            var value = func(publishedContent);
            while (value is Func<DynamicPublishedContent, TOut>)
            {
                value = (value as Func<DynamicPublishedContent, TOut>)(publishedContent);
            }
            //when you're sorting a list of properties
            //and one of those properties doesn't exist, it will come back as DynamicNull
            //when that gets handled by the expression tree parser, it's converted to be false
            //which lets .Where work properly with e.g. umbracoNaviHide which may not be defined
            //on all properties
            //this checks to see if the type of value is bool, and if it is, and it's false
            //then return false
            //in a case where Reduce<bool> is being called, this will return false, which is the 
            //same as the actual real value
            //but when Reduce<object> [a dynamicnode dynamic property call] is called
            //it will result in null, (because default(object) is null) 
            //which will cause the item with the missing property to sort up to the top of the list
            if (value.GetType() == typeof(bool) && !((value as bool?) ?? false))
            {
                return default(TOut);
            }
            if (value.GetType() == typeof(string) && string.IsNullOrEmpty((value as string)))
            {
                return default(TOut);
            }
            return (TOut)value;
        }
		public static IOrderedQueryable<DynamicPublishedContent> OrderBy(object source, object key)
        {
            IEnumerable<DynamicPublishedContent> typedSource = source as IEnumerable<DynamicPublishedContent>;
            LambdaExpression lambda = key as LambdaExpression;
            //if the lambda we have returns an actual property, not a dynamic one,
            //then the TOut of the func will be the actual type, not object
            //Func<DynamicNode, object> func = (Func<DynamicNode, object>)lambda.Compile();
            var func = lambda.Compile();
            var TOut = func.GetType().GetGenericArguments()[1];
            IOrderedQueryable<DynamicPublishedContent> result = null;
            if (TOut == typeof(Func<DynamicPublishedContent, object>))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .OrderBy(x => Reduce<object>(func as Func<DynamicPublishedContent, object>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(object))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .OrderBy(x => Reduce<object>(func as Func<DynamicPublishedContent, object>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(bool))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .OrderBy(x => Reduce<bool>(func as Func<DynamicPublishedContent, bool>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(decimal))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .OrderBy(x => Reduce<decimal>(func as Func<DynamicPublishedContent, decimal>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(int))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .OrderBy(x => Reduce<int>(func as Func<DynamicPublishedContent, int>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(string))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .OrderBy(x => Reduce<string>(func as Func<DynamicPublishedContent, string>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(DateTime))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .OrderBy(x => Reduce<DateTime>(func as Func<DynamicPublishedContent, DateTime>, x))
                    .AsQueryable();
            }
            return result;
        }
		public static IOrderedQueryable<DynamicPublishedContent> ThenBy(object source, object key)
        {
            IOrderedQueryable<DynamicPublishedContent> typedSource = source as IOrderedQueryable<DynamicPublishedContent>;
            LambdaExpression lambda = key as LambdaExpression;
            var func = lambda.Compile();
            var TOut = func.GetType().GetGenericArguments()[1];
            IOrderedQueryable<DynamicPublishedContent> result = null;
            if (TOut == typeof(Func<DynamicPublishedContent, object>))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .ThenBy(x => Reduce<object>(func as Func<DynamicPublishedContent, object>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(object))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .ThenBy(x => Reduce<object>(func as Func<DynamicPublishedContent, object>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(bool))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .ThenBy(x => Reduce<bool>(func as Func<DynamicPublishedContent, bool>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(decimal))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .ThenBy(x => Reduce<decimal>(func as Func<DynamicPublishedContent, decimal>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(int))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .ThenBy(x => Reduce<int>(func as Func<DynamicPublishedContent, int>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(string))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .ThenBy(x => Reduce<string>(func as Func<DynamicPublishedContent, string>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(DateTime))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .ThenBy(x => Reduce<DateTime>(func as Func<DynamicPublishedContent, DateTime>, x))
                    .AsQueryable();
            }
            return result;
        }
		public static IOrderedQueryable<DynamicPublishedContent> OrderByDescending(object source, object key)
        {
            IEnumerable<DynamicPublishedContent> typedSource = source as IEnumerable<DynamicPublishedContent>;
            LambdaExpression lambda = key as LambdaExpression;
            var func = lambda.Compile();
            var TOut = func.GetType().GetGenericArguments()[1];
            IOrderedQueryable<DynamicPublishedContent> result = null;
            if (TOut == typeof(Func<DynamicPublishedContent, object>))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .OrderByDescending(x => Reduce<object>(func as Func<DynamicPublishedContent, object>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(object))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .OrderByDescending(x => Reduce<object>(func as Func<DynamicPublishedContent, object>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(bool))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .OrderByDescending(x => Reduce<bool>(func as Func<DynamicPublishedContent, bool>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(decimal))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .OrderByDescending(x => Reduce<decimal>(func as Func<DynamicPublishedContent, decimal>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(int))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .OrderByDescending(x => Reduce<int>(func as Func<DynamicPublishedContent, int>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(string))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .OrderByDescending(x => Reduce<string>(func as Func<DynamicPublishedContent, string>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(DateTime))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .OrderByDescending(x => Reduce<DateTime>(func as Func<DynamicPublishedContent, DateTime>, x))
                    .AsQueryable();
            }
            return result;
        }
		public static IOrderedQueryable<DynamicPublishedContent> ThenByDescending(object source, object key)
        {
            IOrderedQueryable<DynamicPublishedContent> typedSource = source as IOrderedQueryable<DynamicPublishedContent>;
            LambdaExpression lambda = key as LambdaExpression;
            var func = lambda.Compile();
            var TOut = func.GetType().GetGenericArguments()[1];
            IOrderedQueryable<DynamicPublishedContent> result = null;
            if (TOut == typeof(Func<DynamicPublishedContent, object>))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .ThenByDescending(x => Reduce<object>(func as Func<DynamicPublishedContent, object>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(object))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .ThenByDescending(x => Reduce<object>(func as Func<DynamicPublishedContent, object>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(bool))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .ThenByDescending(x => Reduce<bool>(func as Func<DynamicPublishedContent, bool>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(decimal))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .ThenByDescending(x => Reduce<decimal>(func as Func<DynamicPublishedContent, decimal>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(int))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .ThenByDescending(x => Reduce<int>(func as Func<DynamicPublishedContent, int>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(string))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .ThenByDescending(x => Reduce<string>(func as Func<DynamicPublishedContent, string>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(DateTime))
            {
                result = (IOrderedQueryable<DynamicPublishedContent>)typedSource
                    .ThenByDescending(x => Reduce<DateTime>(func as Func<DynamicPublishedContent, DateTime>, x))
                    .AsQueryable();
            }
            return result;
        }

    }
}

