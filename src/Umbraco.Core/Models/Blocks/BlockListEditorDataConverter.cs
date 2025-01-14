// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Data converter for the block list property editor
/// </summary>
public class BlockListEditorDataConverter : BlockEditorDataConverter<BlockListValue, BlockListLayoutItem>
{
    public BlockListEditorDataConverter(IJsonSerializer jsonSerializer)
        : base(jsonSerializer)
    {
    }

    protected override IEnumerable<ContentAndSettingsReference> GetBlockReferences(IEnumerable<BlockListLayoutItem> layout)
        => layout.Select(x => new ContentAndSettingsReference(x.ContentKey, x.SettingsKey)).ToList();
}
