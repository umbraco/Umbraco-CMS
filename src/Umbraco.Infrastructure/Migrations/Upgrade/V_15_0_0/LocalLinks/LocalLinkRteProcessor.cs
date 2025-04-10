using System.Text.RegularExpressions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

[Obsolete("Will be removed in V18")]
public class LocalLinkRteProcessor : ITypedLocalLinkProcessor
{
    public Type PropertyEditorValueType => typeof(RichTextEditorValue);

    public IEnumerable<string> PropertyEditorAliases =>
    [
        "Umbraco.TinyMCE", Constants.PropertyEditors.Aliases.RichText
    ];

    public Func<object?, Func<object?, bool>, Func<string, string>, bool> Process => ProcessRichText;

    public bool ProcessRichText(
        object? value,
        Func<object?, bool> processNested,
        Func<string, string> processStringValue)
    {
        if (value is not RichTextEditorValue richTextValue)
        {
            return false;
        }

        bool hasChanged = false;

        var newMarkup = processStringValue.Invoke(richTextValue.Markup);

        // fix recursive hickup in ConvertRichTextEditorProperties
        newMarkup = RteBlockHelper.BlockRegex().Replace(
            newMarkup,
            match => UdiParser.TryParse(match.Groups["udi"].Value, out GuidUdi? guidUdi)
                ? match.Value
                    .Replace(match.Groups["attribute"].Value, "data-content-key")
                    .Replace(match.Groups["udi"].Value, guidUdi.Guid.ToString("D"))
                : string.Empty);

        if (newMarkup.Equals(richTextValue.Markup) == false)
        {
            hasChanged = true;
            richTextValue.Markup = newMarkup;
        }

        if (richTextValue.Blocks is null)
        {
            return hasChanged;
        }

        foreach (BlockItemData blockItemData in richTextValue.Blocks.ContentData)
        {
            foreach (BlockPropertyValue blockPropertyValue in blockItemData.Values)
            {
                if (processNested.Invoke(blockPropertyValue.Value))
                {
                    hasChanged = true;
                }
            }
        }

        return hasChanged;
    }
}

[Obsolete("Will be removed in V18")]
public static partial class RteBlockHelper
{
    [GeneratedRegex("<umb-rte-block.*(?<attribute>data-content-udi)=\"(?<udi>.[^\"]*)\".*<\\/umb-rte-block")]
    public static partial Regex BlockRegex();
}
