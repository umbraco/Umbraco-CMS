using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Dynamics
{
    internal static class ExtensionMethods
    {
        public static IEnumerable<TSource> Map<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> selectorFunction,
            Func<TSource, IEnumerable<TSource>> getChildrenFunction)
        {
            if (!source.Any())
            {
                return source;
            }
            // Add what we have to the stack   
            var flattenedList = source.Where(selectorFunction);
            // Go through the input enumerable looking for children,   
            // and add those if we have them   
            foreach (TSource element in source)
            {
                var secondInner = getChildrenFunction(element);
                if (secondInner.Any())
                {
                    secondInner = secondInner.Map(selectorFunction, getChildrenFunction);
                }
                flattenedList = flattenedList.Concat(secondInner);
            }
            return flattenedList;
        }


        public static DynamicDocumentList Random(this DynamicDocumentList all, int min, int max)
        {
            //get a random number generator
            Random r = new Random();
            //choose the number of elements to be returned between Min and Max
            int Number = r.Next(min, max);
            //Call the other method
            return Random(all, Number);
        }
        public static DynamicDocumentList Random(this DynamicDocumentList all, int max)
        {
            //Randomly order the items in the set by a Guid, Take the correct number, and return this wrapped in a new DynamicNodeList
            return new DynamicDocumentList(all.Items.OrderBy(x => Guid.NewGuid()).Take(max));
        }

        public static DynamicDocument Random(this DynamicDocumentList all)
        {
            return all.Items.OrderBy(x => Guid.NewGuid()).First();
        }

        public static bool ContainsAny(this string haystack, IEnumerable<string> needles)
        {
        	if (haystack == null) throw new ArgumentNullException("haystack");
        	if (!string.IsNullOrEmpty(haystack) || needles.Any())
            {
            	return needles.Any(haystack.Contains);
            }
            return false;
        }
        public static bool ContainsAny(this string haystack, params string[] needles)
        {
        	if (haystack == null) throw new ArgumentNullException("haystack");
        	if (!string.IsNullOrEmpty(haystack) || needles.Length > 0)
            {
            	return needles.Any(haystack.Contains);
            }
            return false;
        }
		public static bool ContainsAny(this string haystack, StringComparison comparison, IEnumerable<string> needles)
        {
        	if (haystack == null) throw new ArgumentNullException("haystack");
        	if (!string.IsNullOrEmpty(haystack) || needles.Any())
            {
            	return needles.Any(value => haystack.IndexOf(value, comparison) >= 0);
            }
            return false;
        }
        public static bool ContainsAny(this string haystack, StringComparison comparison, params string[] needles)
        {
        	if (haystack == null) throw new ArgumentNullException("haystack");
        	if (!string.IsNullOrEmpty(haystack) || needles.Length > 0)
            {
            	return needles.Any(value => haystack.IndexOf(value, comparison) >= 0);
            }
            return false;
        }
        public static bool ContainsInsensitive(this string haystack, string needle)
        {
        	if (haystack == null) throw new ArgumentNullException("haystack");
        	return haystack.IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

    	public static bool HasValue(this string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }

    }
}
