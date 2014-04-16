using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Core.Dynamics
{
    [Obsolete("This class should not be used, it is just referenced by already obsoleted code and will be removed in the future")]
    internal static class ExtensionMethods
    {
        //public static IEnumerable<TSource> Map<TSource>(
        //    this IEnumerable<TSource> source,
        //    Func<TSource, bool> selectorFunction,
        //    Func<TSource, IEnumerable<TSource>> getChildrenFunction)
        //{
        //    if (!source.Any())
        //    {
        //        return source;
        //    }
        //    // Add what we have to the stack   
        //    var flattenedList = source.Where(selectorFunction);
        //    // Go through the input enumerable looking for children,   
        //    // and add those if we have them   
        //    foreach (TSource element in source)
        //    {
        //        var secondInner = getChildrenFunction(element);
        //        if (secondInner.Any())
        //        {
        //            secondInner = secondInner.Map(selectorFunction, getChildrenFunction);
        //        }
        //        flattenedList = flattenedList.Concat(secondInner);
        //    }
        //    return flattenedList;
        //}

        [Obsolete("This method should not be used and will be removed in the future")]
        public static bool ContainsAny(this string haystack, IEnumerable<string> needles)
        {
            return StringExtensions.ContainsAny(haystack, needles);
        }
        [Obsolete("This method should not be used and will be removed in the future")]
        public static bool ContainsAny(this string haystack, params string[] needles)
        {
        	return StringExtensions.ContainsAny(haystack, needles);
        }
        [Obsolete("This method should not be used and will be removed in the future")]
		public static bool ContainsAny(this string haystack, StringComparison comparison, IEnumerable<string> needles)
        {
            return StringExtensions.ContainsAny(haystack, needles, comparison);
        }
        [Obsolete("This method should not be used and will be removed in the future")]
        public static bool ContainsAny(this string haystack, StringComparison comparison, params string[] needles)
        {
            return StringExtensions.ContainsAny(haystack, needles, comparison);
        }
        [Obsolete("This method should not be used and will be removed in the future")]
        public static bool ContainsInsensitive(this string haystack, string needle)
        {
        	if (haystack == null) throw new ArgumentNullException("haystack");
        	return haystack.IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
        [Obsolete("This method should not be used and will be removed in the future")]
    	public static bool HasValue(this string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }

    }
}
