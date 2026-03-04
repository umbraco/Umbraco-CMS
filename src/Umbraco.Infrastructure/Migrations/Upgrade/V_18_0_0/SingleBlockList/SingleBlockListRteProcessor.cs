using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

[Obsolete("Scheduled for removal in Umbraco 22.")] // available in v17, activated in v18 migration needs to work on LTS to LTS 17=>21
internal class SingleBlockListRteProcessor : SingleBlockBlockProcessorBase, ITypedSingleBlockListProcessor
{
    public SingleBlockListRteProcessor(SingleBlockListConfigurationCache blockListConfigurationCache)
        : base(blockListConfigurationCache)
    {
    }

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

        foreach (BlockItemData contentData in richTextValue.Blocks.ContentData)
        {
            if (ProcessBlockItemDataValues(contentData, processNested, processOuterValue))
            {
                hasChanged = true;
            }
        }

        foreach (BlockItemData settingsData in richTextValue.Blocks.SettingsData)
        {
            if (ProcessBlockItemDataValues(settingsData, processNested, processOuterValue))
            {
                hasChanged = true;
            }
        }

        return hasChanged;
    }
}
