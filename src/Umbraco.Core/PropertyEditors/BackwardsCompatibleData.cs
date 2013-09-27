using System;
using System.Xml;
using umbraco.interfaces;

namespace Umbraco.Core.PropertyEditors
{

    /// <summary>
    /// This is used purelty to attempt to maintain some backwards compatibility with new property editors that don't have a 
    /// legacy property editor predecessor when developers are using the legacy APIs
    /// </summary>
    internal class BackwardsCompatibleData : IData
    {
        public int PropertyId { set; get; }
        
        public object Value { get; set; }


        public XmlNode ToXMl(XmlDocument data)
        {
            //TODO: We need to get the xml property value converters in place, then this method will need to call in to that converter to 
            // get the xml, for now we're just creating a CDATA section with the raw value.

            var sValue = Value != null ? Value.ToString() : String.Empty;
            return data.CreateCDataSection(sValue);

        }
        
        public void MakeNew(int PropertyId)
        {
            //DO nothing
        }

        public void Delete()
        {
            throw new NotSupportedException(
                    typeof(IData)
                    + " is a legacy object and is not supported by runtime generated "
                    + " instances to maintain backwards compatibility with the legacy APIs. Consider upgrading your code to use the new Services APIs.");
        }
    }
}