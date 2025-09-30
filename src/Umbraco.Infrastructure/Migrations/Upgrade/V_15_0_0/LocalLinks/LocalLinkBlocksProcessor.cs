using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_0_0.LocalLinks;

[Obsolete("Will be removed in V18")]
public abstract class LocalLinkBlocksProcessor
{
    public bool ProcessBlocks(
        object? value,
        Func<object?, bool> processNested,
        Func<string, string> processStringValue)
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

[Obsolete("Will be removed in V18")]
public class LocalLinkBlockListProcessor : LocalLinkBlocksProcessor, ITypedLocalLinkProcessor
{
    public Type PropertyEditorValueType => typeof(BlockListValue);

    public IEnumerable<string> PropertyEditorAliases => [Constants.PropertyEditors.Aliases.BlockList];

    public Func<object?, Func<object?, bool>, Func<string, string>, bool> Process => ProcessBlocks;
}

[Obsolete("Will be removed in V18")]
public class LocalLinkBlockGridProcessor : LocalLinkBlocksProcessor, ITypedLocalLinkProcessor
{
    public Type PropertyEditorValueType => typeof(BlockGridValue);

    public IEnumerable<string> PropertyEditorAliases => [Constants.PropertyEditors.Aliases.BlockGrid];

    public Func<object?, Func<object?, bool>, Func<string, string>, bool> Process => ProcessBlocks;
}
