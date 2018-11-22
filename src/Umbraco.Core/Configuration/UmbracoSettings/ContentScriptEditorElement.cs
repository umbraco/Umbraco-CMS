using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentScriptEditorElement : UmbracoConfigurationElement
    {
        [ConfigurationProperty("scriptFolderPath")]
        internal InnerTextConfigurationElement<string> ScriptFolderPath
        {
            get { return GetOptionalTextElement("scriptFolderPath", "/scripts"); }
        }

        [ConfigurationProperty("scriptFileTypes")]
        internal OptionalCommaDelimitedConfigurationElement ScriptFileTypes
        {
            get { return GetOptionalDelimitedElement("scriptFileTypes", new[] {"js", "xml"}); }
        }

        [ConfigurationProperty("scriptDisableEditor")]
        internal InnerTextConfigurationElement<bool> ScriptEditorDisable
        {
            get { return GetOptionalTextElement("scriptDisableEditor", false); }
        }

    }
}
