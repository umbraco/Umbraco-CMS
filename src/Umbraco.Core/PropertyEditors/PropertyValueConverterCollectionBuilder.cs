using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class PropertyValueConverterCollectionBuilder : OrderedCollectionBuilderBase<PropertyValueConverterCollectionBuilder, PropertyValueConverterCollection, IPropertyValueConverter>
    {
        public PropertyValueConverterCollectionBuilder(IContainer container)
            : base(container)
        { }

        protected override PropertyValueConverterCollectionBuilder This => this;
    }
}
