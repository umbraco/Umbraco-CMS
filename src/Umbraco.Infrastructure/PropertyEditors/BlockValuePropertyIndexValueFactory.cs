// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

internal sealed class BlockValuePropertyIndexValueFactory :
    NestedPropertyIndexValueFactoryBase<BlockValuePropertyIndexValueFactory.IndexValueFactoryBlockValue, BlockItemData>,
    IBlockValuePropertyIndexValueFactory
{
    public BlockValuePropertyIndexValueFactory(
        PropertyEditorCollection propertyEditorCollection,
        IJsonSerializer jsonSerializer,
        IOptionsMonitor<IndexingSettings> indexingSettings)
        : base(propertyEditorCollection, jsonSerializer, indexingSettings)
    {
    }

    protected override IContentType? GetContentTypeOfNestedItem(BlockItemData input, IDictionary<Guid, IContentType> contentTypeDictionary)
        => contentTypeDictionary.TryGetValue(input.ContentTypeKey, out var result) ? result : null;

    protected override IDictionary<string, object?> GetRawProperty(BlockItemData blockItemData) =>
        blockItemData.RawPropertyValues;

    protected override IEnumerable<BlockItemData> GetDataItems(IndexValueFactoryBlockValue input) => input.ContentData;

    // we only care about the content data when extracting values for indexing - not the layouts nor the settings
    internal class IndexValueFactoryBlockValue
    {
        public List<BlockItemData> ContentData { get; set; } = new();
    }
}
