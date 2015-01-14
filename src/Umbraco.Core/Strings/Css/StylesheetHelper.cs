using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Umbraco.Core.Strings.Css
{
    internal class StylesheetHelper
    {
        private const string RuleRegexFormat = @"/\*\s*name:\s*(?<Name>{0}?)\s*\*/\s*(?<Selector>[^\s,{{]*?)\s*{{\s*(?<Styles>.*?)\s*}}";

        public static IEnumerable<StylesheetRule> ParseRules(string input)
        {
            var rules = new List<StylesheetRule>();
            var ruleRegex = new Regex(string.Format(RuleRegexFormat, @"[^\*\r\n]*"), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var contents = input;
            var ruleMatches = ruleRegex.Matches(contents);

            foreach (Match match in ruleMatches)
            {
                rules.Add(new StylesheetRule
                {
                    //RuleId = new HiveId(new Uri("storage://stylesheets"), string.Empty, new HiveIdValue(input.Id.Value + "/" + match.Groups["Name"].Value)),
                    //StylesheetId = input.Id,
                    
                    Name = match.Groups["Name"].Value,
                    Selector = match.Groups["Selector"].Value,
                    // Only match first selector when chained together
                    Styles = string.Join(Environment.NewLine, match.Groups["Styles"].Value.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).Select(x => x.Trim()).ToArray())
                });
            }

            return rules;
        }

        public static string ReplaceRule(string input, string oldRuleName, StylesheetRule rule)
        {
            var contents = input;
            var ruleRegex = new Regex(string.Format(RuleRegexFormat, oldRuleName), RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
            contents = ruleRegex.Replace(contents, rule != null ? rule.ToString() : "");
            return contents;
        }

        public static string AppendRule(string input, StylesheetRule rule)
        {
            var contents = input;
            contents += Environment.NewLine + Environment.NewLine + rule;
            return contents;
        }
    }
}
