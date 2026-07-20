using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

[Obsolete("Scheduled for removal in Umbraco 22.")] // Available in v17, activated in v18. Migration needs to work on LTS to LTS 17=>21
internal class SingleBlockListBlockListProcessor : SingleBlockBlockProcessorBase, ITypedSingleBlockListProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleBlockListBlockListProcessor"/> class.
    /// </summary>
    /// <param name="blockListConfigurationCache">Cache used to store and retrieve block list configurations.</param>
    public SingleBlockListBlockListProcessor(
        SingleBlockListConfigurationCache blockListConfigurationCache)
        : base(blockListConfigurationCache)
    {
    }

    /// <summary>
    /// Gets the type of the property editor value, which is always <see cref="BlockListValue"/>.
    /// </summary>
    public Type PropertyEditorValueType => typeof(BlockListValue);

    /// <summary>
    /// Gets the collection of property editor aliases that this processor supports, typically used to identify Block List property editors handled by the <see cref="SingleBlockListBlockListProcessor"/>.
    /// </summary>
    public IEnumerable<string> PropertyEditorAliases => [Constants.PropertyEditors.Aliases.BlockList];

    /// <summary>
    /// Gets a function that processes blocks in a block list.
    /// The function takes an input object, a predicate to filter blocks, and a selector for block values,
    /// and returns a boolean indicating whether the processing was successful.
    /// </summary>
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
