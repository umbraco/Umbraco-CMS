using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

[Obsolete("Will be removed in V22")] // available in v17, activated in v18 migration needs to work on LTS to LTS 17=>21
public class SingleBlockListRteProcessor : ITypedSingleBlockListProcessor
{
    public Type PropertyEditorValueType => typeof(RichTextEditorValue);

    public IEnumerable<string> PropertyEditorAliases =>
    [
        "Umbraco.TinyMCE", Constants.PropertyEditors.Aliases.RichText
    ];

    public Func<object?, Func<object?, bool>,Func<BlockListValue,object>, bool> Process => ProcessRichText;

    public bool ProcessRichText(
        object? value,
        Func<object?, bool> processNested,
        Func<BlockListValue, object> processOuterValue)
    {
        if (value is not RichTextEditorValue richTextValue)
        {
            return false;
        }

        var hasChanged = false;

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
