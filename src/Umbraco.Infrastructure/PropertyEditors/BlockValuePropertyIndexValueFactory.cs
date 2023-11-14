// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

internal sealed class BlockValuePropertyIndexValueFactory :
    NestedPropertyIndexValueFactoryBase<BlockValue, BlockItemData>,
    IBlockValuePropertyIndexValueFactory
{
    private readonly IContentTypeService _contentTypeService;

    public BlockValuePropertyIndexValueFactory(
        PropertyEditorCollection propertyEditorCollection,
        IContentTypeService contentTypeService,
        IJsonSerializer jsonSerializer,
        IOptionsMonitor<IndexingSettings> indexingSettings)
        : base(propertyEditorCollection, jsonSerializer, indexingSettings)
    {
        _contentTypeService = contentTypeService;
    }

    [Obsolete("Use non-obsolete constructor. This will be removed in Umbraco 14.")]
    public BlockValuePropertyIndexValueFactory(
        PropertyEditorCollection propertyEditorCollection,
        IContentTypeService contentTypeService,
        IJsonSerializer jsonSerializer)
        : this(propertyEditorCollection, contentTypeService, jsonSerializer, StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<IndexingSettings>>())
    {
        _contentTypeService = contentTypeService;
    }

    [Obsolete("Use non-obsolete overload, scheduled for removal in v14")]
    protected override IContentType? GetContentTypeOfNestedItem(BlockItemData input) =>
        _contentTypeService.Get(input.ContentTypeKey);

    protected override IContentType? GetContentTypeOfNestedItem(BlockItemData input, IDictionary<Guid, IContentType> contentTypeDictionary)
        => contentTypeDictionary.TryGetValue(input.ContentTypeKey, out var result) ? result : null;

    protected override IDictionary<string, object?> GetRawProperty(BlockItemData blockItemData) =>
        blockItemData.RawPropertyValues;

    protected override IEnumerable<BlockItemData> GetDataItems(BlockValue input) => input.ContentData;
}
