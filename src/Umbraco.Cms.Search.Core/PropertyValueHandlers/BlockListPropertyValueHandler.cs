using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.PropertyValueHandlers.Collection;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Indexes block list property values by recursively indexing the contained blocks' content.
/// </summary>
internal sealed class BlockListPropertyValueHandler : BlockEditorPropertyValueHandler, ICorePropertyValueHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlockListPropertyValueHandler"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer used to deserialize the block property's stored value.</param>
    /// <param name="contentTypeService">The service used to resolve the contained blocks' element types.</param>
    /// <param name="elementService">The service used to resolve externally referenced (reusable) elements.</param>
    /// <param name="propertyEditorCollection">The property editor collection used to resolve each contained property's editor.</param>
    /// <param name="propertyValueHandlerCollection">The property value handler collection used to index each contained property's value.</param>
    /// <param name="indexingSettings">The indexing settings, used to determine whether external element content should be flattened into the index.</param>
    /// <param name="logger">The logger used to record diagnostic information when indexing blocks.</param>
    public BlockListPropertyValueHandler(
        IJsonSerializer jsonSerializer,
        IContentTypeService contentTypeService,
        IElementService elementService,
        PropertyEditorCollection propertyEditorCollection,
        PropertyValueHandlerCollection propertyValueHandlerCollection,
        IOptions<IndexingSettings> indexingSettings,
        ILogger<BlockListPropertyValueHandler> logger)
        : base(jsonSerializer, contentTypeService, elementService, propertyEditorCollection, propertyValueHandlerCollection, indexingSettings, logger)
    {
    }

    /// <inheritdoc />
    public override bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.BlockList;

    /// <inheritdoc />
    protected override BlockValue? ParseBlockValue(IProperty property, string? culture, string? segment, bool published)
        => ParseBlockValue<BlockListValue>(property, culture, segment, published);
}
