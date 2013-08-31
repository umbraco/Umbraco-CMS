using System;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class RazorStaticMappingElement : InnerTextConfigurationElement<string>
    {     
        internal Guid DataTypeGuid
        {
            get
            {
                return RawXml.Attribute("dataTypeGuid") == null
                           ? default(Guid)
                           : Guid.Parse(RawXml.Attribute("dataTypeGuid").Value);
            }
        }

        internal string NodeTypeAlias
        {
            get
            {
                return RawXml.Attribute("nodeTypeAlias") == null
                           ? null
                           : RawXml.Attribute("nodeTypeAlias").Value;
            }
        }

        internal string DocumentTypeAlias
        {
            get
            {
                return RawXml.Attribute("documentTypeAlias") == null
                           ? null
                           : RawXml.Attribute("documentTypeAlias").Value;
            }
        }

    }
}