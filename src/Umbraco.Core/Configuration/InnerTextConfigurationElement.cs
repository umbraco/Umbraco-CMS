using System;
using System.Xml;
using System.Xml.Linq;

namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// A full config section is required for any full element and we have some elements that are defined like this:
    /// {element}MyValue{/element} instead of as attribute values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class InnerTextConfigurationElement<T> : RawXmlConfigurationElement
    {
        public InnerTextConfigurationElement()
        {
        }

        public InnerTextConfigurationElement(XElement rawXml) : base(rawXml)
        {
        }

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            base.DeserializeElement(reader, serializeCollectionKey);
            //now validate and set the raw value
            if (RawXml.HasElements)
                throw new InvalidOperationException("An InnerTextConfigurationElement cannot contain any child elements, only attributes and a value");
            RawValue = RawXml.Value.Trim();

            //RawValue = reader.ReadElementContentAsString();
        }

        public virtual T Value
        {
            get
            {
                var converted = RawValue.TryConvertTo<T>();
                if (converted.Success == false)
                    throw new InvalidCastException("Could not convert value " + RawValue + " to type " + typeof(T));
                return converted.Result;
            }
        }

        /// <summary>
        /// Exposes the raw string value
        /// </summary>
        internal string RawValue { get; set; }

        /// <summary>
        /// Implicit operator so we don't need to use the 'Value' property explicitly
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static implicit operator T(InnerTextConfigurationElement<T> m)
        {
            return m.Value;
        }

        /// <summary>
        /// Return the string value of Value
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}", Value);
        }

    }
}