using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace umbraco.MacroEngines
{
    public static class ExtensionMethods
    {
        public static IEnumerable<TSource> Map<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> selectorFunction,
            Func<TSource, IEnumerable<TSource>> getChildrenFunction)
        {
            // Add what we have to the stack   
            var flattenedList = source.Where(selectorFunction);
            // Go through the input enumerable looking for children,   
            // and add those if we have them   
            foreach (TSource element in source)
            {
                flattenedList = flattenedList.Concat(getChildrenFunction(element).Map(selectorFunction, getChildrenFunction));
            }
            return flattenedList;
        }


        public static DynamicNodeList Random(this DynamicNodeList all, int Min, int Max)
        {
            //get a random number generator
            Random r = new Random();
            //choose the number of elements to be returned between Min and Max
            int Number = r.Next(Min, Max);
            //Call the other method
            return Random(all, Number);
        }
        public static DynamicNodeList Random(this DynamicNodeList all, int Max)
        {
            //Randomly order the items in the set by a Guid, Take the correct number, and return this wrapped in a new DynamicNodeList
            return new DynamicNodeList(all.Items.OrderBy(x => Guid.NewGuid()).Take(Max));
        }

        public static DynamicNode Random(this DynamicNodeList all)
        {
            return all.Items.OrderBy(x => Guid.NewGuid()).First();
        }

        public static bool ContainsAny(this string haystack, List<string> needles)
        {
            if (!string.IsNullOrEmpty(haystack) || needles.Count > 0)
            {
                foreach (string value in needles)
                {
                    if (haystack.Contains(value))
                        return true;
                }
            }
            return false;
        }
        public static bool ContainsAny(this string haystack, params string[] needles)
        {
            if (!string.IsNullOrEmpty(haystack) || needles.Length > 0)
            {
                foreach (string value in needles)
                {
                    if (haystack.Contains(value))
                        return true;
                }
            }
            return false;
        }
        public static bool ContainsAny(this string haystack, StringComparison comparison, List<string> needles)
        {
            if (!string.IsNullOrEmpty(haystack) || needles.Count > 0)
            {
                foreach (string value in needles)
                {
                    if (haystack.IndexOf(value, comparison) >= 0)
                        return true;
                }
            }
            return false;
        }
        public static bool ContainsAny(this string haystack, StringComparison comparison, params string[] needles)
        {
            if (!string.IsNullOrEmpty(haystack) || needles.Length > 0)
            {
                foreach (string value in needles)
                {
                    if (haystack.IndexOf(value, comparison) >= 0)
                        return true;
                }
            }
            return false;
        }
        public static bool ContainsInsensitive(this string haystack, string needle)
        {
            return haystack.IndexOf(needle, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        public static bool HasValue(this string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }

    }
}
