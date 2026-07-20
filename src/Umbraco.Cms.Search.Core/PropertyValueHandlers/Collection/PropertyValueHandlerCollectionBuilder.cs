using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers.Collection;

internal sealed class PropertyValueHandlerCollectionBuilder
    : LazyCollectionBuilderBase<PropertyValueHandlerCollectionBuilder, PropertyValueHandlerCollection, IPropertyValueHandler>
{
    protected override PropertyValueHandlerCollectionBuilder This => this;
}
