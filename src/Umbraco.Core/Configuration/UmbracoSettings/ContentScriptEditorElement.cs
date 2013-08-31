using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class ContentScriptEditorElement : ConfigurationElement
    {
        [ConfigurationProperty("scriptFolderPath")]
        internal InnerTextConfigurationElement<string> ScriptFolderPath
        {
            get { return (InnerTextConfigurationElement<string>)this["scriptFolderPath"]; }
        }

        [ConfigurationProperty("scriptFileTypes")]
        internal CommaDelimitedConfigurationElement ScriptFileTypes
        {
            get { return (CommaDelimitedConfigurationElement)this["scriptFileTypes"]; }
        }

        [ConfigurationProperty("scriptDisableEditor")]
        internal InnerTextConfigurationElement<bool> DisableScriptEditor
        {
            get { return (InnerTextConfigurationElement<bool>)this["scriptDisableEditor"]; }
        }
    }
}