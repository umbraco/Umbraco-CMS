using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

[Obsolete("Will be removed in V18")]
public class LocalLinkRteProcessor : ITypedLocalLinkProcessor
{
    public Type PropertyEditorValueType => typeof(RichTextEditorValue);

    public IEnumerable<string> PropertyEditorAliases =>
    [
        Constants.PropertyEditors.Aliases.TinyMce, Constants.PropertyEditors.Aliases.RichText
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
