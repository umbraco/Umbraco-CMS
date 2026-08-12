using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
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
    public BlockListPropertyValueHandler(
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
        => propertyType.PropertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.BlockList;
}
