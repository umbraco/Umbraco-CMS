using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.SingleBlockList;

[Obsolete("Will be removed in V22")] // Available in v17, activated in v18. Migration needs to work on LTS to LTS 17=>21
public class SingleBlockListProcessor
{
    private readonly IEnumerable<ITypedSingleBlockListProcessor> _processors;

    public SingleBlockListProcessor(IEnumerable<ITypedSingleBlockListProcessor> processors) => _processors = processors;

    public IEnumerable<string> GetSupportedPropertyEditorAliases() =>
        _processors.SelectMany(p => p.PropertyEditorAliases);

    public bool ProcessToEditorValue(object? editorValue)
    {
        ITypedSingleBlockListProcessor? processor =
            _processors.FirstOrDefault(p => p.PropertyEditorValueType == editorValue?.GetType());

        return processor is not null && processor.Process.Invoke(editorValue, ProcessToEditorValue, ConvertBlockListToSingleBlock);
    }

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
