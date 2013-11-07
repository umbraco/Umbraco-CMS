using System;
using System.Xml;
using Umbraco.Core.Models;
using umbraco.interfaces;

namespace Umbraco.Core.PropertyEditors
{

    /// <summary>
    /// This is used purelty to attempt to maintain some backwards compatibility with new property editors that don't have a 
    /// legacy property editor predecessor when developers are using the legacy APIs
    /// </summary>
    internal class BackwardsCompatibleData : IData
    {
        private readonly string _propertyEditorAlias;

        public BackwardsCompatibleData(string propertyEditorAlias)
        {
            _propertyEditorAlias = propertyEditorAlias;
        }

        public int PropertyId { set; get; }
        
        public object Value { get; set; }


        public XmlNode ToXMl(XmlDocument data)
        {
            //we'll get the property editor by alias, if it exists (which it absolutely should), then we'll have to create a 
            // fake 'Property' object and pass it to the ConvertDbToXml method so we can get the correct XML fragment that 
            // it needs to make.
            var propertyEditor = PropertyEditorResolver.Current.GetByAlias(_propertyEditorAlias);
            if (propertyEditor != null)
            {
                //create a 'fake' property - we will never know the actual db type here so we'll just make it nvarchar, this shouldn't
                // make any difference for the conversion process though.
                var property = new Property(new PropertyType(_propertyEditorAlias, DataTypeDatabaseType.Nvarchar))
                    {
                        Id = PropertyId,
                        Value = Value
                    };
                var xd = new XmlDocument();
                var xml = propertyEditor.ValueEditor.ConvertDbToXml(property);
                xml.GetXmlNode(xd);

                // return the XML node.
                return data.ImportNode(xd.DocumentElement, true);
            }

            //if for some reason the prop editor wasn't found we'll default to returning the string value in a CDATA block.
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