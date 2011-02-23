using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace umbraco.MacroEngines
{
    public static class DynamicNodeListOrdering
    {
        private static object Reduce(Func<DynamicNode, object> func, DynamicNode node)
        {
            var value = func(node);
            while (value is Func<DynamicNode, object>)
            {
                value = (value as Func<DynamicNode, object>)(node);
            }
            return value;
        }
        public static IOrderedQueryable<DynamicNode> OrderBy(object source, object key)
        {
            IEnumerable<DynamicNode> typedSource = source as IEnumerable<DynamicNode>;
            LambdaExpression lambda = key as LambdaExpression;
            Func<DynamicNode, object> func = (Func<DynamicNode, object>)lambda.Compile();
            IOrderedQueryable<DynamicNode> result = (IOrderedQueryable<DynamicNode>)typedSource.OrderBy(x => Reduce(func, x)).AsQueryable();
            return result;
        }
        public static IOrderedQueryable<DynamicNode> ThenBy(object source, object key)
        {
            IOrderedQueryable<DynamicNode> typedSource = source as IOrderedQueryable<DynamicNode>;
            LambdaExpression lambda = key as LambdaExpression;
            Func<DynamicNode, object> func = (Func<DynamicNode, object>)lambda.Compile();
            IOrderedQueryable<DynamicNode> result = (IOrderedQueryable<DynamicNode>)typedSource.ThenBy(x => Reduce(func, x)).AsQueryable();
            return result;
        }
        public static IOrderedQueryable<DynamicNode> OrderByDescending(object source, object key)
        {
            IEnumerable<DynamicNode> typedSource = source as IEnumerable<DynamicNode>;
            LambdaExpression lambda = key as LambdaExpression;
            Func<DynamicNode, object> func = (Func<DynamicNode, object>)lambda.Compile();
            IOrderedQueryable<DynamicNode> result = (IOrderedQueryable<DynamicNode>)typedSource.OrderByDescending(x => Reduce(func, x)).AsQueryable();
            return result;
        }
        public static IOrderedQueryable<DynamicNode> ThenByDescending(object source, object key)
        {
            IOrderedQueryable<DynamicNode> typedSource = source as IOrderedQueryable<DynamicNode>;
            LambdaExpression lambda = key as LambdaExpression;
            Func<DynamicNode, object> func = (Func<DynamicNode, object>)lambda.Compile();
            IOrderedQueryable<DynamicNode> result = (IOrderedQueryable<DynamicNode>)typedSource.ThenByDescending(x => Reduce(func, x)).AsQueryable();
            return result;
        }

    }
}

