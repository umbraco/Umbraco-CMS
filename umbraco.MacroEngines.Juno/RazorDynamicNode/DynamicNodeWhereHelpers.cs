using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.MacroEngines
{
    public static class DynamicNodeWhereHelpers
    {
        public static bool ContainsAny(string haystack, List<string> needles)
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
        public static bool ContainsAny(string haystack, params string[] needles)
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
        public static bool ContainsAny(string haystack, StringComparison comparison, List<string> needles)
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
        public static bool ContainsAny(string haystack, StringComparison comparison, params string[] needles)
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
    }
}
