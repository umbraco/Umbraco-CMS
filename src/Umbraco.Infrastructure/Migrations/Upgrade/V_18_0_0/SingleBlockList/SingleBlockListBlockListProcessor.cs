using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

[Obsolete("Will be removed in V22")] // Available in v17, activated in v18. Migration needs to work on LTS to LTS 17=>21
public class SingleBlockListBlockListProcessor : SingleBlockBlockProcessorBase, ITypedSingleBlockListProcessor
{
    public SingleBlockListBlockListProcessor(
        SingleBlockListConfigurationCache blockListConfigurationCache)
        : base(blockListConfigurationCache)
    {
    }

    public Type PropertyEditorValueType => typeof(BlockListValue);

    public IEnumerable<string> PropertyEditorAliases => [Constants.PropertyEditors.Aliases.BlockList];

    public Func<object?, Func<object?, bool>, Func<BlockListValue, object>, bool> Process => ProcessBlocks;

    private bool ProcessBlocks(
        object? value,
        Func<object?, bool> processNested,
        Func<BlockListValue, object> processOuterValue)
    {
        if (value is not BlockListValue blockValue)
        {
            return false;
        }

        bool hasChanged = false;

        // there might be another list inside the single list so more recursion, yeeey!
        foreach (BlockItemData contentData in blockValue.ContentData)
        {
            if (ProcessBlockItemDataValues(contentData, processNested, processOuterValue))
            {
                hasChanged = true;
            }
        }

        foreach (BlockItemData settingsData in blockValue.SettingsData)
        {
            if (ProcessBlockItemDataValues(settingsData, processNested, processOuterValue))
            {
                hasChanged = true;
            }
        }

        return hasChanged;
    }
}
