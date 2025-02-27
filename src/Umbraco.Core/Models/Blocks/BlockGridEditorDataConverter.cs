// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
/// Data converter for the block grid property editor
/// </summary>
public class BlockGridEditorDataConverter : BlockEditorDataConverter<BlockGridValue, BlockGridLayoutItem>
{
    public BlockGridEditorDataConverter(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    protected override IEnumerable<ContentAndSettingsReference> GetBlockReferences(IEnumerable<BlockGridLayoutItem> layout)
    {
        IList<ContentAndSettingsReference> ExtractContentAndSettingsReferences(BlockGridLayoutItem item)
        {
            var references = new List<ContentAndSettingsReference> { new(item.ContentKey, item.SettingsKey) };
            references.AddRange(item.Areas.SelectMany(area => area.Items.SelectMany(ExtractContentAndSettingsReferences)));
            return references;
        }

        ContentAndSettingsReference[] result = layout.SelectMany(ExtractContentAndSettingsReferences).ToArray();
        return result;
    }
}
