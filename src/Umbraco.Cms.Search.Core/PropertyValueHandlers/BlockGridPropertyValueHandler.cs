using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.PropertyValueHandlers.Collection;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Indexes block grid property values by recursively indexing the contained blocks' content.
/// </summary>
internal sealed class BlockGridPropertyValueHandler : BlockEditorPropertyValueHandler, ICorePropertyValueHandler
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlockGridPropertyValueHandler"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer used to deserialize the block property's stored value.</param>
    /// <param name="contentTypeService">The service used to resolve the contained blocks' element types.</param>
    /// <param name="propertyEditorCollection">The property editor collection used to resolve each contained property's editor.</param>
    /// <param name="propertyValueHandlerCollection">The property value handler collection used to index each contained property's value.</param>
    /// <param name="logger">The logger used to record diagnostic information when indexing blocks.</param>
    public BlockGridPropertyValueHandler(
        IJsonSerializer jsonSerializer,
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        PropertyValueHandlerCollection propertyValueHandlerCollection,
        ILogger<BlockListPropertyValueHandler> logger)
        : base(jsonSerializer, contentTypeService, propertyEditorCollection, propertyValueHandlerCollection, logger)
    {
    }

    /// <inheritdoc />
    public override bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.BlockGrid;
}
