// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
/// Data converter for the block grid property editor
/// </summary>
public class BlockGridEditorDataConverter : BlockEditorDataConverter<BlockGridValue, BlockGridLayoutItem>
{
    [Obsolete("Use the constructor that takes IJsonSerializer. Will be removed in V15.")]
    public BlockGridEditorDataConverter()
        : this(StaticServiceProvider.Instance.GetRequiredService<IJsonSerializer>())
    {
    }

    public BlockGridEditorDataConverter(IJsonSerializer jsonSerializer)
        : base(Constants.PropertyEditors.Aliases.BlockGrid, jsonSerializer)
    {
    }

    protected override IEnumerable<ContentAndSettingsReference> GetBlockReferences(IEnumerable<BlockGridLayoutItem> layout)
    {
        IList<ContentAndSettingsReference> ExtractContentAndSettingsReferences(BlockGridLayoutItem item)
        {
            var references = new List<ContentAndSettingsReference> { new(item.ContentUdi, item.SettingsUdi) };
            references.AddRange(item.Areas.SelectMany(area => area.Items.SelectMany(ExtractContentAndSettingsReferences)));
            return references;
        }

        ContentAndSettingsReference[] result = layout.SelectMany(ExtractContentAndSettingsReferences).ToArray();
        return result;
    }
}
