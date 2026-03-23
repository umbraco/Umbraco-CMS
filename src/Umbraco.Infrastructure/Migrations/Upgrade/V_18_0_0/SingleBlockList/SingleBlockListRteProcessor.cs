using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

[Obsolete("Scheduled for removal in Umbraco 22.")] // available in v17, activated in v18 migration needs to work on LTS to LTS 17=>21
internal class SingleBlockListRteProcessor : SingleBlockBlockProcessorBase, ITypedSingleBlockListProcessor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleBlockListRteProcessor"/> class.
    /// </summary>
    /// <param name="blockListConfigurationCache">
    /// An instance of <see cref="SingleBlockListConfigurationCache"/> used to retrieve and cache block list configurations for processing rich text editors.
    /// </param>
    public SingleBlockListRteProcessor(SingleBlockListConfigurationCache blockListConfigurationCache)
        : base(blockListConfigurationCache)
    {
    }

    /// <summary>
    /// Gets the type of the property editor value, which is <see cref="RichTextEditorValue"/>.
    /// </summary>
    public Type PropertyEditorValueType => typeof(RichTextEditorValue);

    /// <summary>
    /// Gets the collection of property editor aliases that this processor supports for processing rich text editors in block lists.
    /// </summary>
    public IEnumerable<string> PropertyEditorAliases =>
    [
        "Umbraco.TinyMCE", Constants.PropertyEditors.Aliases.RichText
    ];

    /// <summary>
    /// Gets a delegate that processes rich text editor (RTE) content within a single block list.
    /// The delegate takes an input object, a predicate function, and a transformation function for <see cref="BlockListValue"/>,
    /// and returns a boolean indicating whether the processing was successful.
    /// </summary>
    public Func<object?, Func<object?, bool>,Func<BlockListValue,object>, bool> Process => ProcessRichText;

    /// <summary>
    /// Processes a value expected to be a <see cref="RichTextEditorValue"/> by applying the provided processing functions to its nested and outer block list data.
    /// </summary>
    /// <param name="value">The value to process; should be a <see cref="RichTextEditorValue"/> containing block list data.</param>
    /// <param name="processNested">A function invoked for each nested object within the block list content and settings data.</param>
    /// <param name="processOuterValue">A function applied to each <see cref="BlockListValue"/> found in the block list data.</param>
    /// <returns>
    /// <c>true</c> if any changes were made to the block list data during processing; otherwise, <c>false</c>.
    /// </returns>
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
