using System;
using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Manages the list of PropertyValueConverter's
    /// </summary>
    internal sealed class PropertyValueConvertersResolver : ManyObjectsResolverBase<PropertyValueConvertersResolver, PropertyValueConverter>
    {
        public PropertyValueConvertersResolver(IEnumerable<Type> converters)
            : base(converters)
        {
        }

        public IEnumerable<PropertyValueConverter> Converters
        {
            get { return Values; }
        }
    }
}