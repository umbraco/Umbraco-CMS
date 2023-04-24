using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Strings.Css;

namespace Umbraco.Cms.Core.Services;

public class RichTextStylesheetService : IRichTextStylesheetService
{
    public Task<string> InterpolateRichTextRules(RichTextStylesheetData data)
    {
        // First we remove all existing rules, from the content, in case some of the rules has changed or been removed
        // This way we can simply re-add them.
        StylesheetRule[] existingRules = string.IsNullOrWhiteSpace(data.Content)
            ? Array.Empty<StylesheetRule>()
            : StylesheetHelper.ParseRules(data.Content).ToArray();

        foreach (StylesheetRule rule in existingRules)
        {
            // Setting the rule to null will delete it from the content.
            data.Content = StylesheetHelper.ReplaceRule(data.Content, rule.Name, null);
        }

        data.Content = data.Content?.TrimEnd(Constants.CharArrays.LineFeedCarriageReturn);

        // Now we can re-add all the rules.
        if (data.Rules.Any())
        {
            foreach (StylesheetRule rule in data.Rules)
            {
                data.Content = StylesheetHelper.AppendRule(
                    data.Content,
                    rule);
            }

            data.Content += Environment.NewLine;
        }

        return Task.FromResult(data.Content ?? string.Empty);
    }

    public Task<IEnumerable<StylesheetRule>> ExtractRichTextRules(RichTextStylesheetData data)
    {
        if (string.IsNullOrWhiteSpace(data.Content))
        {
            return Task.FromResult(Enumerable.Empty<StylesheetRule>());
        }

        return Task.FromResult(StylesheetHelper.ParseRules(data.Content));
    }
}
