using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class StringReplaceManyBenchmarks
    {
        private static string text = "1,2.3:4&5#6";

        private static char[] chars = new[] { ',', '.', ':', '&', '#' };

        private static char replacement = '*';

        private static Dictionary<string, string> replacements = new Dictionary<string, string>(5)
        {
            {",", "*" },
            {".", "*" },
            {":", "*" },
            {"&", "*" },
            {"#", "*" },
        };

        [Benchmark(Description = "String.ReplaceMany with Aggregate", Baseline = true)]
        public string ReplaceManyAggregate()
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }

            string result = text;
            return chars.Aggregate(result, (current, c) => current.Replace(c, replacement));
        }

        [Benchmark(Description = "String.ReplaceMany with For Loop")]
        public string ReplaceManyForLoop()
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }

            string result = text;
            for (int i = 0; i < chars.Length; i++)
            {
                result = result.Replace(chars[i], replacement);
            }

            return result;
        }

        [Benchmark(Description = "String.ReplaceMany Dictionary with Aggregate")]
        public string ReplaceManyDictionaryAggregate()
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            if (replacements == null)
            {
                throw new ArgumentNullException("replacements");
            }

            return replacements.Aggregate(text, (current, kvp) => current.Replace(kvp.Key, kvp.Value));
        }

        [Benchmark(Description = "String.ReplaceMany Dictionary with For Each")]
        public string ReplaceManyDictionaryForEach()
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            if (replacements == null)
            {
                throw new ArgumentNullException("replacements");
            }

            string result = text;
            foreach (KeyValuePair<string, string> item in replacements)
            {
                result = result.Replace(item.Key, item.Value);
            }

            return result;
        }
    }
}