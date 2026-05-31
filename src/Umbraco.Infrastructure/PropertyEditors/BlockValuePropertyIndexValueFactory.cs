// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

internal sealed class BlockValuePropertyIndexValueFactory :
    BlockValuePropertyIndexValueFactoryBase<BlockValuePropertyIndexValueFactory.IndexValueFactoryBlockValue>,
    IBlockValuePropertyIndexValueFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlockValuePropertyIndexValueFactory"/> class.
    /// </summary>
    /// <param name="propertyEditorCollection">The <see cref="PropertyEditorCollection"/> containing available property editors.</param>
    /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/> used for serializing and deserializing JSON values.</param>
    /// <param name="indexingSettings">The <see cref="IOptionsMonitor{IndexingSettings}"/> providing access to indexing configuration settings.</param>
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
    internal sealed class IndexValueFactoryBlockValue
    {
        /// <summary>
        /// Gets or sets the list of content block item data.
        /// </summary>
        public List<BlockItemData> ContentData { get; set; } = new();

        /// <summary>
        /// Gets or sets the collection of <see cref="BlockItemVariation"/> instances that should be exposed by the index value factory.
        /// </summary>
        public List<BlockItemVariation> Expose { get; set; } = new();
    }
}
