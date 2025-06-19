// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class BlockValuePropertyIndexValueFactory :
    BlockValuePropertyIndexValueFactoryBase<BlockValuePropertyIndexValueFactory.IndexValueFactoryBlockValue>,
    IBlockValuePropertyIndexValueFactory
{
    public BlockValuePropertyIndexValueFactory(
        PropertyEditorCollection propertyEditorCollection,
        IJsonSerializer jsonSerializer,
        IOptionsMonitor<IndexingSettings> indexingSettings)
        : base(propertyEditorCollection, jsonSerializer, indexingSettings)
    {
    }

    protected override IEnumerable<RawDataItem> GetDataItems(IndexValueFactoryBlockValue input, bool published)
        => GetDataItems(input.ContentData, input.Expose, published);

    // we only care about the content data when extracting values for indexing - not the layouts nor the settings
    internal class IndexValueFactoryBlockValue
    {
        public List<BlockItemData> ContentData { get; set; } = new();

        public List<BlockItemVariation> Expose { get; set; } = new();
    }
}
