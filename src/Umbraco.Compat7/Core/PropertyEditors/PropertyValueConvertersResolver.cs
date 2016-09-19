using System.Collections.Generic;
using CoreCurrent = Umbraco.Core.DependencyInjection.Current;

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.PropertyEditors
{
    public class PropertyValueConvertersResolver
    {
        public static PropertyValueConvertersResolver Current { get; set; } = new PropertyValueConvertersResolver();

        public IEnumerable<IPropertyValueConverter> Converters => CoreCurrent.PropertyValueConverters;

        // fixme - should implement the basic resolver add/edit/etc methods!
    }
}
