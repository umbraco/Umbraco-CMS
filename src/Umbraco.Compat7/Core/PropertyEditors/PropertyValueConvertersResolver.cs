using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;
using CoreCurrent = Umbraco.Core.Composing.Current;
using LightInject;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.PropertyEditors
{
    public class PropertyValueConvertersResolver : ManyObjectsResolverBase<PropertyValueConverterCollectionBuilder, PropertyValueConverterCollection, IPropertyValueConverter>
    {
        private PropertyValueConvertersResolver(PropertyValueConverterCollectionBuilder builder)
            : base(builder)
        { }

        public static PropertyValueConvertersResolver Current { get; }
            = new PropertyValueConvertersResolver(CoreCurrent.Container.GetInstance<PropertyValueConverterCollectionBuilder>());

        public IEnumerable<IPropertyValueConverter> Converters => CoreCurrent.PropertyValueConverters;
    }
}
