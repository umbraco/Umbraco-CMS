using System.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// Used for specifying default values for comma delimited config
    /// </summary>
    internal class OptionalCommaDelimitedConfigurationElement : CommaDelimitedConfigurationElement
    {
        private readonly CommaDelimitedConfigurationElement _wrapped;
        private readonly string[] _defaultValue;

        public OptionalCommaDelimitedConfigurationElement()
        {            
        }

        public OptionalCommaDelimitedConfigurationElement(CommaDelimitedConfigurationElement wrapped, string[] defaultValue)
        {
            _wrapped = wrapped;
            _defaultValue = defaultValue;
        }

        public override CommaDelimitedStringCollection Value
        {
            get
            {
                if (_wrapped == null)
                {
                    return base.Value;
                }

                if (string.IsNullOrEmpty(_wrapped.RawValue))
                {
                    var val = new CommaDelimitedStringCollection();
                    val.AddRange(_defaultValue);
                    return val;
                }
                return _wrapped.Value;
            }
        }
    }
}