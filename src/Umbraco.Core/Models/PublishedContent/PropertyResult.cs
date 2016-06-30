using System;
using System.Web;

namespace Umbraco.Core.Models.PublishedContent
{
    // fixme - temp
    internal class PropertyResult : IPublishedProperty, IHtmlString
    {
        private readonly IPublishedProperty _source;
	    private readonly string _alias;
	    private readonly object _value;

        internal PropertyResult(IPublishedProperty source, PropertyResultType type)
        {
    		if (source == null) throw new ArgumentNullException(nameof(source));
            
            PropertyType = type;
            _source = source;
        }

		internal PropertyResult(string alias, object value, PropertyResultType type)
        {
        	if (alias == null) throw new ArgumentNullException(nameof(alias));
        	if (value == null) throw new ArgumentNullException(nameof(value));

            PropertyType = type;
            _alias = alias;
			_value = value;
        }

        internal PropertyResultType PropertyType { get; }

        public string PropertyTypeAlias => _source == null ? _alias : _source.PropertyTypeAlias;
        public object SourceValue => _source == null ? _value : _source.SourceValue;
        public bool HasValue => _source == null || _source.HasValue;
        public object Value => _source == null ? _value : _source.Value;
        public object XPathValue => Value?.ToString();

        public string ToHtmlString()
        {
            var value = Value;
			return value?.ToString() ?? string.Empty;
        }
    }
}
