using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
/// Data converter for the block grid property editor
/// </summary>
public class BlockGridEditorDataConverter : BlockEditorDataConverter
{
    private readonly IJsonSerializer _jsonSerializer;

    public BlockGridEditorDataConverter(IJsonSerializer jsonSerializer) : base(Cms.Core.Constants.PropertyEditors.Aliases.BlockGrid)
        => _jsonSerializer = jsonSerializer;

    protected override IEnumerable<ContentAndSettingsReference>? GetBlockReferences(JToken jsonLayout)
    {
        IEnumerable<BlockGridLayoutItem>? blockListLayouts = jsonLayout.ToObject<IEnumerable<BlockGridLayoutItem>>();
        if (blockListLayouts == null)
        {
            return null;
        }

        var result = new List<ContentAndSettingsReference>();

        foreach (BlockGridLayoutItem blockGridLayoutItem in blockListLayouts)
        {
            AddToResult(blockGridLayoutItem, result);
        }

        return result;
    }

    private void AddToResult(BlockGridLayoutItem layoutItem, List<ContentAndSettingsReference> result)
    {
        result.Add(new ContentAndSettingsReference(layoutItem.ContentUdi, layoutItem.SettingsUdi));

        foreach (BlockGridLayoutItem areaLayoutItem in layoutItem.Areas.SelectMany(x => x.Items))
        {
            AddToResult(areaLayoutItem, result);
        }
    }
}
