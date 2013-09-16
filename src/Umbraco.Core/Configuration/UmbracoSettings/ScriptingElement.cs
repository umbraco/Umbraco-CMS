using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ScriptingElement : ConfigurationElement, IScripting
    {
        [ConfigurationProperty("razor")]
        internal RazorElement Razor
        {
            get { return (RazorElement) base["razor"]; }
        }

        IEnumerable<INotDynamicXmlDocument> IScripting.NotDynamicXmlDocumentElements
        {
            get { return Razor.NotDynamicXmlDocumentElements; }
        }

        IEnumerable<IRazorStaticMapping> IScripting.DataTypeModelStaticMappings
        {
            get { return Razor.DataTypeModelStaticMappings; }
        }
    }
}