using System;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using Match = System.Text.RegularExpressions.Match;

namespace Umbraco.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class HtmlUrlParserBenchmarks
    {
        private static readonly Regex ResolveUrlPattern = new Regex("(=[\"\']?)(\\W?\\~(?:.(?![\"\']?\\s+(?:\\S+)=|[>\"\']))+.)[\"\']?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        private static readonly string Text = "~/Test";

        [Benchmark(Baseline = true)]
        public void EnsureUrls()
        {
            var text = Text;
            var tags = ResolveUrlPattern.Matches(Text);
            foreach (Match tag in tags)
            {
                var url = "";
                if (tag.Groups[1].Success)
                    url = tag.Groups[1].Value;

                // The richtext editor inserts a slash in front of the URL. That's why we need this little fix
                //                if (url.StartsWith("/"))
                //                    text = text.Replace(url, ResolveUrl(url.Substring(1)));
                //                else
                if (string.IsNullOrEmpty(url) == false)
                {
                    var resolvedUrl = (url.Substring(0, 1) == "/") ? url.Substring(1) : url;
                    text = text.Replace(url, resolvedUrl);
                }
            }
        }

        [Benchmark]
        public void EnsureUrlsTest()
        {
            var text = Text;
            foreach (Match tag in ResolveUrlPattern.Matches(Text))
            {
                if (!tag.Groups[1].Success) continue;

                var url = tag.Groups[1].Value.AsSpan();

                // The richtext editor inserts a slash in front of the URL. That's why we need this little fix
                //                if (url.StartsWith("/"))
                //                    text = text.Replace(url, ResolveUrl(url.Substring(1)));
                //                else
                var resolvedUrl = (url.Slice(0, 1) == "/") ? url[1..] : url;
                text = text.Replace(url.ToString(), resolvedUrl.ToString());
            }
        }
    }
}
