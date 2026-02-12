using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

[Obsolete("Scheduled for removal in Umbraco 22.")] // Available in v17, activated in v18. Migration needs to work on LTS to LTS 17=>21
public class SingleBlockListProcessor
{
    private readonly IEnumerable<ITypedSingleBlockListProcessor> _processors;

    public SingleBlockListProcessor(IEnumerable<ITypedSingleBlockListProcessor> processors) => _processors = processors;

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
    /// Updates and returns the passed in BlockListValue to a SingleBlockValue
    /// Should only be called by a core processor once a BlockListValue has been found that is configured in single block mode.
    /// </summary>
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
