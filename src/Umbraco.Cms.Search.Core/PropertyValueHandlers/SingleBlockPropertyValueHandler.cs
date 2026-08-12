using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
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
    public SingleBlockPropertyValueHandler(
        IJsonSerializer jsonSerializer,
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        PropertyValueHandlerCollection propertyValueHandlerCollection,
        ILogger<SingleBlockPropertyValueHandler> logger)
        : base(jsonSerializer, contentTypeService, propertyEditorCollection, propertyValueHandlerCollection, logger)
    {
    }

    /// <inheritdoc />
    public override bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Umbraco.Cms.Core.Constants.PropertyEditors.Aliases.SingleBlock;
}
