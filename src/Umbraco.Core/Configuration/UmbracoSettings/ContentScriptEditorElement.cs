using System.Collections.Generic;
using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentScriptEditorElement : ConfigurationElement
    {
        [ConfigurationProperty("scriptFolderPath")]
        internal InnerTextConfigurationElement<string> ScriptFolderPath
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<string>(
                       (InnerTextConfigurationElement<string>)this["scriptFolderPath"],
                    //set the default
                       "/scripts");
            }
        }

        [ConfigurationProperty("scriptFileTypes")]
        internal OptionalCommaDelimitedConfigurationElement ScriptFileTypes
        {
            get
            {
                return new OptionalCommaDelimitedConfigurationElement(
                       (OptionalCommaDelimitedConfigurationElement)this["scriptFileTypes"],
                    //set the default
                       new[] { "js", "xml" });                
            }
        }

        [ConfigurationProperty("scriptDisableEditor")]
        internal InnerTextConfigurationElement<bool> ScriptEditorDisable
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>) this["scriptDisableEditor"],
                    //set the default
                    false);
            }
        }

    }
}