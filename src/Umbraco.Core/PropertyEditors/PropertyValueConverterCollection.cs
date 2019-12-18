using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class PropertyValueConverterCollection : BuilderCollectionBase<IPropertyValueConverter>
    {
        public PropertyValueConverterCollection(IEnumerable<IPropertyValueConverter> items)
            : base(items)
        { }

        private readonly object _locker = new object();
        private Dictionary<IPropertyValueConverter, Type[]> _defaultConverters;

        private Dictionary<IPropertyValueConverter, Type[]> DefaultConverters
        {
            get
            {
                lock (_locker)
                {
                    if (_defaultConverters != null)
                        return _defaultConverters;

                    _defaultConverters = new Dictionary<IPropertyValueConverter, Type[]>();

                    foreach (var converter in this)
                    {
                        var attr = converter.GetType().GetCustomAttribute<DefaultPropertyValueConverterAttribute>(false);
                        if (attr != null)
                            _defaultConverters[converter] = attr.DefaultConvertersToShadow;
                    }

                    return _defaultConverters;
                }
            }
        }

        internal bool IsDefault(IPropertyValueConverter converter)
            => DefaultConverters.ContainsKey(converter);

        internal bool Shadows(IPropertyValueConverter shadowing, IPropertyValueConverter shadowed)
            => DefaultConverters.TryGetValue(shadowing, out Type[] types) && types.Contains(shadowed.GetType());
    }
}
