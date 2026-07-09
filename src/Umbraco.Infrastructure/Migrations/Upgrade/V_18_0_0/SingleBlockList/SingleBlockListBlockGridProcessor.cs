using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

[Obsolete("Scheduled for removal in Umbraco 22.")] // Available in v17, activated in v18. Migration needs to work on LTS to LTS 17=>21
internal class SingleBlockListBlockGridProcessor : SingleBlockBlockProcessorBase, ITypedSingleBlockListProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleBlockListBlockGridProcessor"/> class, used for processing block grid data during the upgrade to version 18.0.0.
    /// </summary>
    /// <param name="blockListConfigurationCache">An instance of <see cref="SingleBlockListConfigurationCache"/> used to retrieve block list configuration data.</param>
    public SingleBlockListBlockGridProcessor(SingleBlockListConfigurationCache blockListConfigurationCache)
        : base(blockListConfigurationCache)
    {
    }

    /// <summary>
    /// Gets the type of the property editor value, which is always <see cref="BlockGridValue"/>.
    /// </summary>
    public Type PropertyEditorValueType => typeof(BlockGridValue);

    /// <summary>
    /// Gets the collection of property editor aliases that this processor supports for block grid processing.
    /// Typically, this will include the alias for the Block Grid property editor.
    /// </summary>
    public IEnumerable<string> PropertyEditorAliases => [Constants.PropertyEditors.Aliases.BlockGrid];

    /// <summary>
    /// Gets a delegate that processes block list values using the specified predicate and selector functions.
    /// </summary>
    /// <remarks>
    /// The returned function takes an input object, a predicate to filter items, and a selector to project <see cref="BlockListValue"/> objects, returning a boolean result.
    /// </remarks>
    public Func<object?, Func<object?, bool>,Func<BlockListValue,object>, bool> Process => ProcessBlocks;

    private bool ProcessBlocks(
        object? value,
        Func<object?, bool> processNested,
        Func<BlockListValue, object> processOuterValue)
    {
        if (value is not BlockGridValue blockValue)
        {
            return false;
        }

        bool hasChanged = false;

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
