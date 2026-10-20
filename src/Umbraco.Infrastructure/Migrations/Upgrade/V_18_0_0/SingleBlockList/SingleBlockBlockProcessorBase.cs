using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

[Obsolete("Scheduled for removal in Umbraco 22.")] // Available in v17, activated in v18. Migration needs to work on LTS to LTS 17=>21
internal abstract class SingleBlockBlockProcessorBase
{
    private readonly SingleBlockListConfigurationCache _blockListConfigurationCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleBlockBlockProcessorBase"/> class.
    /// </summary>
    /// <param name="blockListConfigurationCache">Cache containing block list configurations.</param>
    public SingleBlockBlockProcessorBase(
        SingleBlockListConfigurationCache blockListConfigurationCache)
    {
        _blockListConfigurationCache = blockListConfigurationCache;
    }

    protected bool ProcessBlockItemDataValues(
        BlockItemData blockItemData,
        Func<object?, bool> processNested,
        Func<BlockListValue, object> processOuterValue)
    {
        var hasChanged = false;

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
                // A value holding no block is returned unconverted, and must not be flagged as changed: that would
                // have the migration rewrite the row and run the value through a value editor round trip for nothing.
                var convertedValue = processOuterValue.Invoke(blockListValue);
                if (ReferenceEquals(convertedValue, blockListValue) is false)
                {
                    blockPropertyValue.Value = convertedValue;
                    hasChanged = true;
                }
            }
        }

        return hasChanged;
    }
}
