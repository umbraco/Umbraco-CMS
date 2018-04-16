using LightInject;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class PropertyValueConverterCollectionBuilder : OrderedCollectionBuilderBase<PropertyValueConverterCollectionBuilder, PropertyValueConverterCollection, IPropertyValueConverter>
    {
        public PropertyValueConverterCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override PropertyValueConverterCollectionBuilder This => this;
    }
}
