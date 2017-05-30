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
        private Tuple<IPropertyValueConverter, DefaultPropertyValueConverterAttribute>[] _defaults;

        /// <summary>
        /// Gets the default converters and associated metadata.
        /// </summary>
        internal Tuple<IPropertyValueConverter, DefaultPropertyValueConverterAttribute>[] DefaultConverters
        {
            get
            {
                lock (_locker)
                {
                    return _defaults ?? (_defaults = this.Select(x =>
                        {
                            var attrib = x.GetType().GetCustomAttribute<DefaultPropertyValueConverterAttribute>(false);
                            return attrib == null ? null : Tuple.Create(x, attrib);
                        }).WhereNotNull().ToArray());
                }
            }
        }
    }
}
