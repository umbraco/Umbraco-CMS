using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class PropertyValueConverterCollectionBuilder : OrderedCollectionBuilderBase<PropertyValueConverterCollectionBuilder, PropertyValueConverterCollection, IPropertyValueConverter>
    {
        protected override PropertyValueConverterCollectionBuilder This => this;
    }
}
