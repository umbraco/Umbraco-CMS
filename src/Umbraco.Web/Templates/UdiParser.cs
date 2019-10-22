using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core;

namespace Umbraco.Web.Templates
{
    /// <summary>
    /// Parses out UDIs in strings
    /// </summary>
    public sealed class UdiParser
    {
        private static readonly Regex DataUdiAttributeRegex = new Regex(@"data-udi=\\?(?:""|')(?<udi>umb://[A-z0-9\-]+/[A-z0-9]+)\\?(?:""|')",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        /// <summary>
        /// Parses out UDIs from an html string based on 'data-udi' html attributes
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public IEnumerable<Udi> ParseUdisFromDataAttributes(string text)
        {
            var matches = DataUdiAttributeRegex.Matches(text);
            if (matches.Count == 0)
                yield break;

            foreach (Match match in matches)
            {
                if (match.Groups.Count == 2 && Udi.TryParse(match.Groups[1].Value, out var udi))
                    yield return udi;
            }
        }
    }
}
