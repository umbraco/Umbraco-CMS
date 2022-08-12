// Copyright (c) Umbraco.
// See LICENSE for more details.

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

        IList<ContentAndSettingsReference> ExtractContentAndSettingsReferences(BlockGridLayoutItem item)
        {
            var references = new List<ContentAndSettingsReference> { new(item.ContentUdi, item.SettingsUdi) };
            references.AddRange(item.Areas.SelectMany(area => area.Items.SelectMany(ExtractContentAndSettingsReferences)));
            return references;
        }

        ContentAndSettingsReference[] result = blockListLayouts.SelectMany(ExtractContentAndSettingsReferences).ToArray();
        return result;
    }
}
