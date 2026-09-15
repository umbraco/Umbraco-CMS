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
/// Indexes single block property values by recursively indexing the block's content.
/// </summary>
internal sealed class SingleBlockPropertyValueHandler : BlockEditorPropertyValueHandler, ICorePropertyValueHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SingleBlockPropertyValueHandler"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer used to deserialize the block property's stored value.</param>
    /// <param name="contentTypeService">The service used to resolve the contained block's element type.</param>
    /// <param name="elementService">The service used to resolve externally referenced (reusable) elements.</param>
    /// <param name="propertyEditorCollection">The property editor collection used to resolve each contained property's editor.</param>
    /// <param name="propertyValueHandlerCollection">The property value handler collection used to index each contained property's value.</param>
    /// <param name="indexingSettings">The indexing settings, used to determine whether external element content should be flattened into the index.</param>
    /// <param name="logger">The logger used to record diagnostic information when indexing the block.</param>
    public SingleBlockPropertyValueHandler(
        IJsonSerializer jsonSerializer,
        IContentTypeService contentTypeService,
        IElementService elementService,
        PropertyEditorCollection propertyEditorCollection,
        PropertyValueHandlerCollection propertyValueHandlerCollection,
        IOptions<IndexingSettings> indexingSettings,
        ILogger<SingleBlockPropertyValueHandler> logger)
        : base(jsonSerializer, contentTypeService, elementService, propertyEditorCollection, propertyValueHandlerCollection, indexingSettings, logger)
    {
    }

    /// <inheritdoc />
    public override bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.SingleBlock;

    /// <inheritdoc />
    protected override BlockValue? ParseBlockValue(IProperty property, string? culture, string? segment, bool published)
        => ParseBlockValue<SingleBlockValue>(property, culture, segment, published);
}
