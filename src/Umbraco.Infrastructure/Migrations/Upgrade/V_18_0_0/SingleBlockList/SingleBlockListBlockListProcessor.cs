using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

[Obsolete("Will be removed in V22")] // Available in v17, activated in v18. Migration needs to work on LTS to LTS 17=>21
public class SingleBlockListBlockListProcessor : ITypedSingleBlockListProcessor
{
    private readonly SingleBlockListConfigurationCache _blockListConfigurationCache;

    public SingleBlockListBlockListProcessor(SingleBlockListConfigurationCache blockListConfigurationCache)
    {
        _blockListConfigurationCache = blockListConfigurationCache;
    }

    public Type PropertyEditorValueType => typeof(BlockListValue);

    public IEnumerable<string> PropertyEditorAliases => [Constants.PropertyEditors.Aliases.BlockList];

    public Func<object?, Func<object?, bool>, Func<BlockListValue,object>, bool> Process => ProcessBlocks;

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
        foreach (BlockItemData blockItemData in blockValue.ContentData)
        {
            foreach (BlockPropertyValue blockPropertyValue in blockItemData.Values)
            {
                if (processNested.Invoke(blockPropertyValue.Value))
                {
                    hasChanged = true;
                }

                if (_blockListConfigurationCache.IsPropertyEditorBlockListConfiguredAsSingle(
                        blockPropertyValue.PropertyType!.DataTypeKey)
                    && blockPropertyValue.Value is BlockListValue blockListValue)
                {
                    blockPropertyValue.Value = processOuterValue.Invoke(blockListValue);
                    hasChanged = true;
                }
            }
        }

        return hasChanged;
    }
}
