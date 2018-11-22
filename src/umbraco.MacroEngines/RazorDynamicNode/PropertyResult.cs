using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces;
using umbraco.cms.businesslogic.property;
using System.Web;

namespace umbraco.MacroEngines
{
    public class PropertyResult : IProperty, IHtmlString
    {
        private readonly string _alias;
        private readonly string _value;

        public PropertyResult(IProperty source)
        {
            if (source == null) return;

            _alias = source.Alias;
            _value = source.Value;
        }

        public PropertyResult(string alias, string value)
        {
            _alias = alias;
            _value = value;
        }

        public PropertyResult(Property source)
        {
            _alias = source.PropertyType.Alias;
            _value = source.Value.ToString();
        }

        public string Alias
        {
            get { return _alias; }
        }

        public string Value
        {
            get { return _value; }
        }
        
        public bool IsNull()
        {
            return Value == null;
        }

        public bool HasValue()
        {
            return !string.IsNullOrWhiteSpace(Value);
        }

        public int ContextId { get; set; }
        public string ContextAlias { get; set; }

        // implements IHtmlString.ToHtmlString
        public string ToHtmlString()
        {
            return Value;
        }
    }
}
