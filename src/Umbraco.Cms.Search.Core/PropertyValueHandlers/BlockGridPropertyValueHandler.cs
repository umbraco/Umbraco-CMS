using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.PropertyValueHandlers.Collection;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

internal sealed class BlockGridPropertyValueHandler : BlockEditorPropertyValueHandler, ICorePropertyValueHandler
{
    public BlockGridPropertyValueHandler(
        IJsonSerializer jsonSerializer,
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        PropertyValueHandlerCollection propertyValueHandlerCollection,
        ILogger<BlockListPropertyValueHandler> logger)
        : base(jsonSerializer, contentTypeService, propertyEditorCollection, propertyValueHandlerCollection, logger)
    {
    }

    public override bool CanHandle(string propertyEditorAlias)
        => propertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.BlockGrid;
}
