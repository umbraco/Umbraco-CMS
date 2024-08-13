// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Data converter for the block list property editor
/// </summary>
public class BlockListEditorDataConverter : BlockEditorDataConverter<BlockListValue, BlockListLayoutItem>
{
    [Obsolete("Use the constructor that takes IJsonSerializer. Will be removed in V15.")]
    public BlockListEditorDataConverter()
        : this(StaticServiceProvider.Instance.GetRequiredService<IJsonSerializer>())
    {
    }

    public BlockListEditorDataConverter(IJsonSerializer jsonSerializer)
        : base(Constants.PropertyEditors.Aliases.BlockList, jsonSerializer)
    {
    }

    protected override IEnumerable<ContentAndSettingsReference> GetBlockReferences(IEnumerable<BlockListLayoutItem> layout)
        => layout.Select(x => new ContentAndSettingsReference(x.ContentUdi, x.SettingsUdi)).ToList();
}
