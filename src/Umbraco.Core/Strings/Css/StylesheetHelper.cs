using System.Text.RegularExpressions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Strings.Css;

public class StylesheetHelper
{
    private const string RuleRegexFormat =
        @"/\*\*\s*umb_name:\s*(?<Name>{0}?)\s*\*/\s*(?<Selector>[^,{{]*?)\s*{{\s*(?<Styles>.*?)\s*}}";

    public static IEnumerable<StylesheetRule> ParseRules(string? input)
    {
        var rules = new List<StylesheetRule>();
        var ruleRegex = new Regex(
            string.Format(RuleRegexFormat, @"[^\*\r\n]*"),
            RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (input is not null)
        {
            var contents = input;
            MatchCollection ruleMatches = ruleRegex.Matches(contents);

            foreach (Match match in ruleMatches)
            {
                var name = match.Groups["Name"].Value;

                // If this name already exists, only use the first one
                if (rules.Any(x => x.Name == name))
                {
                    continue;
                }

                rules.Add(new StylesheetRule
                {
                    Name = match.Groups["Name"].Value,
                    Selector = match.Groups["Selector"].Value,

                    // Only match first selector when chained together
                    Styles = string.Join(
                        Environment.NewLine,
                        match.Groups["Styles"].Value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                            .Select(x => x.Trim()).ToArray()),
                });
            }
        }

        return rules;
    }

    public static string? ReplaceRule(string? input, string oldRuleName, StylesheetRule? rule)
    {
        var contents = input;
        if (contents is not null)
        {
            var ruleRegex = new Regex(
                string.Format(RuleRegexFormat, oldRuleName.EscapeRegexSpecialCharacters()),
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            contents = ruleRegex.Replace(contents, rule != null ? rule.ToString() : string.Empty);
        }

        return contents;
    }

    public static string AppendRule(string? input, StylesheetRule rule)
    {
        var contents = input;
        contents += Environment.NewLine + Environment.NewLine + rule;
        return contents;
    }
}
