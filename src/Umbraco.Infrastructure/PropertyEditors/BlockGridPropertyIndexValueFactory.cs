using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

internal sealed class BlockGridPropertyIndexValueFactory
    : BlockValuePropertyIndexValueFactoryBase<BlockGridValue>, IBlockGridPropertyIndexValueFactory
{
    public BlockGridPropertyIndexValueFactory(
        PropertyEditorCollection propertyEditorCollection,
        IElementService elementService,
        IJsonSerializer jsonSerializer,
        IOptionsMonitor<IndexingSettings> indexingSettings)
        : base(propertyEditorCollection, elementService, jsonSerializer, indexingSettings)
    {
    }

    protected override IEnumerable<RawDataItem> GetDataItems(BlockGridValue input, bool published)
        => GetDataItems(input.GetLayouts() ?? [], input.ContentData, input.Expose, published);
}
