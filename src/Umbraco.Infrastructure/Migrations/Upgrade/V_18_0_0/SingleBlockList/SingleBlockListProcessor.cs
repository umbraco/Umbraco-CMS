using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

/// <summary>
/// Handles the migration logic for converting or updating single block list data structures
/// as part of the upgrade process to version 18.0.0.
/// </summary>
/// <remarks>Available in v17, activated in v18. Migration needs to work on LTS to LTS 17=>21</remarks>
[Obsolete("Scheduled for removal in Umbraco 22.")]
public class SingleBlockListProcessor
{
    private readonly IEnumerable<ITypedSingleBlockListProcessor> _processors;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleBlockListProcessor"/> class.
    /// </summary>
    /// <param name="processors">A collection of <see cref="ITypedSingleBlockListProcessor"/> instances used to process single block lists.</param>
    public SingleBlockListProcessor(IEnumerable<ITypedSingleBlockListProcessor> processors) => _processors = processors;

    /// <summary>
    /// Returns a collection of property editor aliases supported by all registered block list processors.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{String}"/> containing the supported property editor aliases.</returns>
    public IEnumerable<string> GetSupportedPropertyEditorAliases() =>
        _processors.SelectMany(p => p.PropertyEditorAliases);

    /// <summary>
    /// The entry point of the recursive conversion
    /// Find the first processor that can handle the value and call it's Process method
    /// </summary>
    /// <returns>Whether the value was changed</returns>
    public bool ProcessToEditorValue(object? editorValue)
    {
        ITypedSingleBlockListProcessor? processor =
            _processors.FirstOrDefault(p => p.PropertyEditorValueType == editorValue?.GetType());

        return processor is not null && processor.Process.Invoke(editorValue, ProcessToEditorValue, ConvertBlockListToSingleBlock);
    }

    /// <summary>
    /// Converts a <see cref="BlockListValue"/> configured in single block mode to a <see cref="SingleBlockValue"/>.
    /// Should only be called by a core processor after verifying the input is in single block mode.
    /// </summary>
    /// <param name="blockListValue">The <see cref="BlockListValue"/> to convert.</param>
    /// <returns>The resulting <see cref="SingleBlockValue"/> representing the single block.</returns>
    public BlockValue ConvertBlockListToSingleBlock(BlockListValue blockListValue)
    {
        IBlockLayoutItem blockListLayoutItem = blockListValue.Layout[Constants.PropertyEditors.Aliases.BlockList].First();

        var singleBlockLayoutItem = new SingleBlockLayoutItem
        {
            ContentKey = blockListLayoutItem.ContentKey,
            SettingsKey = blockListLayoutItem.SettingsKey,
        };

        var singleBlockValue = new SingleBlockValue(singleBlockLayoutItem)
        {
            ContentData = blockListValue.ContentData,
            SettingsData = blockListValue.SettingsData,
            Expose = blockListValue.Expose,
        };

        return singleBlockValue;
    }
}
