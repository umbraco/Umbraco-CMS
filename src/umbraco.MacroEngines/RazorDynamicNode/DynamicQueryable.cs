//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using umbraco.MacroEngines;
using System.Diagnostics;

namespace System.Linq.Dynamic
{

	[Obsolete("This class has been superceded by Umbraco.Web.Dynamics.DynamicQueryable but is marked internal and shouldn't be used directly in your code")]
    public static class DynamicQueryable
    {
        public static IQueryable<T> Where<T>(this IQueryable<T> source, string predicate, params object[] values)
        {
			return (IQueryable<T>)Umbraco.Web.Dynamics.DynamicQueryable.Where<T>((IQueryable)source, predicate, values);
        }

        public static IQueryable Where(this IQueryable source, string predicate, params object[] values)
        {
			return (IQueryable)Umbraco.Web.Dynamics.DynamicQueryable.Where<DynamicNode>((IQueryable)source, predicate, values);			
        }

        public static IQueryable Select(this IQueryable<DynamicNode> source, string selector, params object[] values)
        {
			return (IQueryable)Umbraco.Web.Dynamics.DynamicQueryable.Select<DynamicNode>(source, selector, values);		
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string ordering, params object[] values)
        {
			return (IQueryable<T>)Umbraco.Web.Dynamics.DynamicQueryable.OrderBy<T>(source, ordering, () => typeof(DynamicNodeListOrdering), values);		
        }

        public static IQueryable OrderBy(this IQueryable source, string ordering, params object[] values)
        {
			return (IQueryable)Umbraco.Web.Dynamics.DynamicQueryable.OrderBy<DynamicNode>(source, ordering, () => typeof(DynamicNodeListOrdering), values);		
        }
        
        public static IQueryable Take(this IQueryable source, int count)
        {
			return (IQueryable)Umbraco.Web.Dynamics.DynamicQueryable.Take(source, count);	
        }

        public static IQueryable Skip(this IQueryable source, int count)
        {
			return (IQueryable)Umbraco.Web.Dynamics.DynamicQueryable.Skip(source, count);	
        }

        public static IQueryable GroupBy(this IQueryable source, string keySelector, string elementSelector, params object[] values)
        {
			return (IQueryable)Umbraco.Web.Dynamics.DynamicQueryable.GroupBy<DynamicNode>(source, keySelector, elementSelector, values);	
        }

        public static bool Any(this IQueryable source)
        {
			return Umbraco.Web.Dynamics.DynamicQueryable.Any(source);	
        }

        public static int Count(this IQueryable source)
        {
			return Umbraco.Web.Dynamics.DynamicQueryable.Count(source);	
        }
    }
}
