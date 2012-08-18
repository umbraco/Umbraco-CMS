using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Umbraco.Core.Dynamics
{
    internal static class DynamicDocumentListOrdering
    {

        private static TOut Reduce<TOut>(Func<DynamicDocument, TOut> func, DynamicDocument document)
        {
            var value = func(document);
            while (value is Func<DynamicDocument, TOut>)
            {
                value = (value as Func<DynamicDocument, TOut>)(document);
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
        public static IOrderedQueryable<DynamicDocument> OrderBy(object source, object key)
        {
            IEnumerable<DynamicDocument> typedSource = source as IEnumerable<DynamicDocument>;
            LambdaExpression lambda = key as LambdaExpression;
            //if the lambda we have returns an actual property, not a dynamic one,
            //then the TOut of the func will be the actual type, not object
            //Func<DynamicNode, object> func = (Func<DynamicNode, object>)lambda.Compile();
            var func = lambda.Compile();
            var TOut = func.GetType().GetGenericArguments()[1];
            IOrderedQueryable<DynamicDocument> result = null;
            if (TOut == typeof(Func<DynamicDocument, object>))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .OrderBy(x => Reduce<object>(func as Func<DynamicDocument, object>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(object))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .OrderBy(x => Reduce<object>(func as Func<DynamicDocument, object>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(bool))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .OrderBy(x => Reduce<bool>(func as Func<DynamicDocument, bool>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(decimal))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .OrderBy(x => Reduce<decimal>(func as Func<DynamicDocument, decimal>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(int))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .OrderBy(x => Reduce<int>(func as Func<DynamicDocument, int>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(string))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .OrderBy(x => Reduce<string>(func as Func<DynamicDocument, string>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(DateTime))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .OrderBy(x => Reduce<DateTime>(func as Func<DynamicDocument, DateTime>, x))
                    .AsQueryable();
            }
            return result;
        }
        public static IOrderedQueryable<DynamicDocument> ThenBy(object source, object key)
        {
            IOrderedQueryable<DynamicDocument> typedSource = source as IOrderedQueryable<DynamicDocument>;
            LambdaExpression lambda = key as LambdaExpression;
            var func = lambda.Compile();
            var TOut = func.GetType().GetGenericArguments()[1];
            IOrderedQueryable<DynamicDocument> result = null;
            if (TOut == typeof(Func<DynamicDocument, object>))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .ThenBy(x => Reduce<object>(func as Func<DynamicDocument, object>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(object))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .ThenBy(x => Reduce<object>(func as Func<DynamicDocument, object>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(bool))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .ThenBy(x => Reduce<bool>(func as Func<DynamicDocument, bool>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(decimal))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .ThenBy(x => Reduce<decimal>(func as Func<DynamicDocument, decimal>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(int))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .ThenBy(x => Reduce<int>(func as Func<DynamicDocument, int>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(string))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .ThenBy(x => Reduce<string>(func as Func<DynamicDocument, string>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(DateTime))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .ThenBy(x => Reduce<DateTime>(func as Func<DynamicDocument, DateTime>, x))
                    .AsQueryable();
            }
            return result;
        }
        public static IOrderedQueryable<DynamicDocument> OrderByDescending(object source, object key)
        {
            IEnumerable<DynamicDocument> typedSource = source as IEnumerable<DynamicDocument>;
            LambdaExpression lambda = key as LambdaExpression;
            var func = lambda.Compile();
            var TOut = func.GetType().GetGenericArguments()[1];
            IOrderedQueryable<DynamicDocument> result = null;
            if (TOut == typeof(Func<DynamicDocument, object>))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .OrderByDescending(x => Reduce<object>(func as Func<DynamicDocument, object>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(object))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .OrderByDescending(x => Reduce<object>(func as Func<DynamicDocument, object>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(bool))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .OrderByDescending(x => Reduce<bool>(func as Func<DynamicDocument, bool>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(decimal))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .OrderByDescending(x => Reduce<decimal>(func as Func<DynamicDocument, decimal>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(int))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .OrderByDescending(x => Reduce<int>(func as Func<DynamicDocument, int>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(string))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .OrderByDescending(x => Reduce<string>(func as Func<DynamicDocument, string>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(DateTime))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .OrderByDescending(x => Reduce<DateTime>(func as Func<DynamicDocument, DateTime>, x))
                    .AsQueryable();
            }
            return result;
        }
        public static IOrderedQueryable<DynamicDocument> ThenByDescending(object source, object key)
        {
            IOrderedQueryable<DynamicDocument> typedSource = source as IOrderedQueryable<DynamicDocument>;
            LambdaExpression lambda = key as LambdaExpression;
            var func = lambda.Compile();
            var TOut = func.GetType().GetGenericArguments()[1];
            IOrderedQueryable<DynamicDocument> result = null;
            if (TOut == typeof(Func<DynamicDocument, object>))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .ThenByDescending(x => Reduce<object>(func as Func<DynamicDocument, object>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(object))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .ThenByDescending(x => Reduce<object>(func as Func<DynamicDocument, object>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(bool))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .ThenByDescending(x => Reduce<bool>(func as Func<DynamicDocument, bool>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(decimal))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .ThenByDescending(x => Reduce<decimal>(func as Func<DynamicDocument, decimal>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(int))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .ThenByDescending(x => Reduce<int>(func as Func<DynamicDocument, int>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(string))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .ThenByDescending(x => Reduce<string>(func as Func<DynamicDocument, string>, x))
                    .AsQueryable();
            }
            if (TOut == typeof(DateTime))
            {
                result = (IOrderedQueryable<DynamicDocument>)typedSource
                    .ThenByDescending(x => Reduce<DateTime>(func as Func<DynamicDocument, DateTime>, x))
                    .AsQueryable();
            }
            return result;
        }

    }
}

