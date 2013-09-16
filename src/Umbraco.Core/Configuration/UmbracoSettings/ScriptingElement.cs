using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ScriptingElement : ConfigurationElement, IScriptingSection
    {
        [ConfigurationProperty("razor")]
        internal RazorElement Razor
        {
            get { return (RazorElement) base["razor"]; }
        }

        IEnumerable<INotDynamicXmlDocument> IScriptingSection.NotDynamicXmlDocumentElements
        {
            get { return Razor.NotDynamicXmlDocumentElements; }
        }

        IEnumerable<IRazorStaticMapping> IScriptingSection.DataTypeModelStaticMappings
        {
            get { return Razor.DataTypeModelStaticMappings; }
        }
    }
}