// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

internal sealed class BlockValuePropertyIndexValueFactory :
    NestedPropertyIndexValueFactoryBase<BlockValue, BlockItemData>,
    IBlockValuePropertyIndexValueFactory
{
    private readonly IContentTypeService _contentTypeService;


    public BlockValuePropertyIndexValueFactory(
        PropertyEditorCollection propertyEditorCollection,
        IContentTypeService contentTypeService,
        IJsonSerializer jsonSerializer)
        : base(propertyEditorCollection, jsonSerializer)
    {
        _contentTypeService = contentTypeService;
    }


    protected override IContentType? GetContentTypeOfNestedItem(BlockItemData input) =>
        _contentTypeService.Get(input.ContentTypeKey);

    protected override IDictionary<string, object?> GetRawProperty(BlockItemData blockItemData) =>
        blockItemData.RawPropertyValues;

    protected override IEnumerable<BlockItemData> GetDataItems(BlockValue input) => input.ContentData;
}
