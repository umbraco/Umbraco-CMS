// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

internal sealed class SingleBlockPropertyIndexValueFactory
    : BlockValuePropertyIndexValueFactoryBase<SingleBlockValue>, ISingleBlockPropertyIndexValueFactory
{
    public SingleBlockPropertyIndexValueFactory(
        PropertyEditorCollection propertyEditorCollection,
        IElementService elementService,
        IJsonSerializer jsonSerializer,
        IOptionsMonitor<IndexingSettings> indexingSettings)
        : base(propertyEditorCollection, elementService, jsonSerializer, indexingSettings)
    {
    }

    protected override IEnumerable<RawDataItem> GetDataItems(SingleBlockValue input, bool published)
        => GetDataItems(input.GetLayouts() ?? [], input.ContentData, input.Expose, published);
}
