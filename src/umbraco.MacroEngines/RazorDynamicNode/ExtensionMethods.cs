using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Core;

namespace umbraco.MacroEngines
{
    public static class ExtensionMethods
    {
        [Obsolete("This has been superceded by Umbraco.Core.EnumerableExtensions.FlattenList method")]
        public static IEnumerable<TSource> Map<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> selectorFunction,
            Func<TSource, IEnumerable<TSource>> getChildrenFunction)
        {
		    return source.SelectRecursive(getChildrenFunction).Where(selectorFunction);
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

		[Obsolete("This has been superceded by Umbraco.Core.Dynamics.ExtensionMethods.ContainsAny method")]
        public static bool ContainsAny(this string haystack, List<string> needles)
		{
			return Umbraco.Core.Dynamics.ExtensionMethods.ContainsAny(haystack, needles);
		}

		[Obsolete("This has been superceded by Umbraco.Core.Dynamics.ExtensionMethods.ContainsAny method")]
        public static bool ContainsAny(this string haystack, params string[] needles)
        {
			return Umbraco.Core.Dynamics.ExtensionMethods.ContainsAny(haystack, needles);
        }

		[Obsolete("This has been superceded by Umbraco.Core.Dynamics.ExtensionMethods.ContainsAny method")]
        public static bool ContainsAny(this string haystack, StringComparison comparison, List<string> needles)
        {
			return Umbraco.Core.Dynamics.ExtensionMethods.ContainsAny(haystack, comparison, needles);
        }

		[Obsolete("This has been superceded by Umbraco.Core.Dynamics.ExtensionMethods.ContainsAny method")]
        public static bool ContainsAny(this string haystack, StringComparison comparison, params string[] needles)
        {
			return Umbraco.Core.Dynamics.ExtensionMethods.ContainsAny(haystack, comparison, needles);
        }

		[Obsolete("This has been superceded by Umbraco.Core.Dynamics.ExtensionMethods.ContainsInsensitive method")]
        public static bool ContainsInsensitive(this string haystack, string needle)
        {
			return Umbraco.Core.Dynamics.ExtensionMethods.ContainsInsensitive(haystack, needle);
        }

		[Obsolete("This has been superceded by Umbraco.Core.Dynamics.ExtensionMethods.HasValue method")]
        public static bool HasValue(this string s)
        {
			return Umbraco.Core.Dynamics.ExtensionMethods.HasValue(s);
        }

    }
}
