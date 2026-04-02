using System.Text.RegularExpressions;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Strings.Css;

/// <summary>
///     Provides helper methods for parsing and manipulating Umbraco-style CSS stylesheet rules.
/// </summary>
/// <remarks>
///     FIXME: remove this class and all usage of it (moved to the client in V14).
/// </remarks>
public class StylesheetHelper
{
    /// <summary>
    ///     The regular expression format for matching Umbraco stylesheet rules.
    /// </summary>
    private const string RuleRegexFormat =
        @"/\*\*\s*umb_name:\s*(?<Name>{0}?)\s*\*/\s*(?<Selector>[^,{{]*?)\s*{{\s*(?<Styles>.*?)\s*}}";

    /// <summary>
    ///     Parses stylesheet rules from a CSS string.
    /// </summary>
    /// <param name="input">The CSS content to parse.</param>
    /// <returns>A collection of <see cref="StylesheetRule"/> objects extracted from the input.</returns>
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
                        match.Groups["Styles"].Value.Split(new[] { "\r\n", "\n" }, StringSplitOptions.TrimEntries)),
                });
            }
        }

        return rules;
    }

    /// <summary>
    ///     Replaces a stylesheet rule in the CSS content.
    /// </summary>
    /// <param name="input">The CSS content containing the rule to replace.</param>
    /// <param name="oldRuleName">The name of the rule to replace.</param>
    /// <param name="rule">The new rule to insert, or <c>null</c> to remove the rule.</param>
    /// <returns>The modified CSS content with the rule replaced or removed.</returns>
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

    /// <summary>
    ///     Appends a stylesheet rule to the CSS content.
    /// </summary>
    /// <param name="input">The existing CSS content.</param>
    /// <param name="rule">The rule to append.</param>
    /// <returns>The CSS content with the new rule appended.</returns>
    public static string AppendRule(string? input, StylesheetRule rule)
    {
        var contents = input;
        contents += Environment.NewLine + Environment.NewLine + rule;
        return contents;
    }
}
