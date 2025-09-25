using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

[Obsolete("Will be removed in V22")] // Available in v17, activated in v18. Migration needs to work on LTS to LTS 17=>21
public abstract class SingleBlockListBlocksProcessor
{
    public bool ProcessBlocks(
        object? value,
        Func<object?, bool> processNested,
        Func<BlockListValue, SingleBlockValue> processValue)
    {
        if (value is not BlockValue blockValue)
        {
            return false;
        }

        bool hasChanged = false;

        foreach (BlockItemData blockItemData in blockValue.ContentData)
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

[Obsolete("Will be removed in V22")] // Available in v17, activated in v18. Migration needs to work on LTS to LTS 17=>21
public class SingleBlockListBlockListProcessor : SingleBlockListBlocksProcessor, ITypedSingleBlockListProcessor
{
    public Type PropertyEditorValueType => typeof(BlockListValue);

    public IEnumerable<string> PropertyEditorAliases => [Constants.PropertyEditors.Aliases.BlockList];

    public Func<object?, Func<object?, bool>, Func<BlockListValue, SingleBlockValue>, bool> Process => ProcessBlocks;
}

[Obsolete("Will be removed in V22")] // Available in v17, activated in v18. Migration needs to work on LTS to LTS 17=>21
public class SingleBlockListBlockGridProcessor : SingleBlockListBlocksProcessor, ITypedSingleBlockListProcessor
{
    public Type PropertyEditorValueType => typeof(BlockGridValue);

    public IEnumerable<string> PropertyEditorAliases => [Constants.PropertyEditors.Aliases.BlockGrid];

    public Func<object?, Func<object?, bool>, Func<BlockListValue, SingleBlockValue>, bool> Process => ProcessBlocks;
}
