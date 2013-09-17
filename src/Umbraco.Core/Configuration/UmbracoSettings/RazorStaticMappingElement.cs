using System;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class RazorStaticMappingElement : InnerTextConfigurationElement<string>, IRazorStaticMapping
    {
        public Guid DataTypeGuid
        {
            get
            {
                return RawXml.Attribute("dataTypeGuid") == null
                           ? default(Guid)
                           : Guid.Parse(RawXml.Attribute("dataTypeGuid").Value);
            }
        }

        public string NodeTypeAlias
        {
            get
            {
                return RawXml.Attribute("nodeTypeAlias") == null
                           ? null
                           : RawXml.Attribute("nodeTypeAlias").Value;
            }
        }

        public string PropertyTypeAlias
        {
            get
            {
                return RawXml.Attribute("propertyTypeAlias") == null
                           ? null
                           : RawXml.Attribute("propertyTypeAlias").Value;
            }
        }

        string IRazorStaticMapping.MappingName
        {
            get { return Value; }
        }
    }
}