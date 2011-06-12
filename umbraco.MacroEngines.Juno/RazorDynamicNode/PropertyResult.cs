using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces;
using umbraco.cms.businesslogic.property;

namespace umbraco.MacroEngines
{
    public class PropertyResult : IProperty
    {
        private string _alias;
        private string _value;
        private Guid _version;

        public PropertyResult(IProperty source)
        {
            this._alias = source.Alias;
            this._value = source.Value;
            this._version = source.Version;
        }
        public PropertyResult(string alias, string value, Guid version)
        {
            this._alias = alias;
            this._value = value;
            this._version = version;
        }
        public PropertyResult(Property source)
        {
            this._alias = source.PropertyType.Alias;
            this._value = string.Format("{0}", source.Value);
            this._version = source.VersionId;
        }
        public string Alias
        {
            get { return _alias; }
        }

        public string Value
        {
            get { return _value; }
        }

        public Guid Version
        {
            get { return _version; }
        }
    }
}
