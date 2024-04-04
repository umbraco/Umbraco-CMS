// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

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

    [Obsolete("Use constructor that doesn't take IContentTypeService, scheduled for removal in V15")]
    public BlockValuePropertyIndexValueFactory(
        PropertyEditorCollection propertyEditorCollection,
        IContentTypeService contentTypeService,
        IJsonSerializer jsonSerializer,
        IOptionsMonitor<IndexingSettings> indexingSettings)
        : this(propertyEditorCollection, jsonSerializer, indexingSettings)
    {
    }

    protected override IContentType? GetContentTypeOfNestedItem(BlockItemData input, IDictionary<Guid, IContentType> contentTypeDictionary)
        => contentTypeDictionary.TryGetValue(input.ContentTypeKey, out var result) ? result : null;

    protected override IDictionary<string, object?> GetRawProperty(BlockItemData blockItemData) =>
        blockItemData.RawPropertyValues;

    protected override IEnumerable<BlockItemData> GetDataItems(IndexValueFactoryBlockValue input) => input.ContentData;

    internal class IndexValueFactoryBlockValue : BlockValue<IndexValueFactoryBlockLayoutItem>
    {
    }

    internal class IndexValueFactoryBlockLayoutItem : IBlockLayoutItem
    {
        public Udi? ContentUdi { get; set; }

        public Udi? SettingsUdi { get; set; }
    }
}
